using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Text.Json.Serialization;
using Townsharp.Infra.Alta.Api;
using Townsharp.Infra.Alta.Api.Identity;
using Townsharp.Infra.Alta.Configuration;
using Townsharp.Infra.Alta.Json;
using Websocket.Client;
using Websocket.Client.Models;

namespace Townsharp.Infra.Alta.Subscriptions
{
    public class SubscriptionClient : IDisposable
    {
        public readonly static Uri SubscriptionWebsocketUri = new Uri("wss://5wx2mgoj95.execute-api.ap-southeast-2.amazonaws.com/dev");
        public readonly static TimeSpan ResponseTimeout = TimeSpan.FromSeconds(15);

        // Dependencies
        private readonly AccountsTokenClient tokenClient;
        private readonly AltaClientConfiguration clientConfiguration;
        private readonly ILogger<SubscriptionClient> logger;

        // WebSocket state
        private bool disposedValue = false;
        private int messageId = 1;
        private State state = State.Disconnected;
        private WebsocketClient? client;
        private string? authToken;
        private ConcurrentDictionary<string, ConcurrentBag<string>> subscriptions = new ConcurrentDictionary<string, ConcurrentBag<string>>();

        // Scheduling and synchronization
        private SemaphoreSlim creatingWebsocket = new SemaphoreSlim(1, 1);
        private TaskCompletionSource migrationTaskSource = new TaskCompletionSource();

        // Subscription Handlers
        // NOTE: make this public and subscribe on generics with a handler method.
        public IObservable<GroupServerStatusMessage> GroupStatusChanged => GroupStatusChangedSubject;
        public readonly Subject<GroupServerStatusMessage> GroupStatusChangedSubject = new Subject<GroupServerStatusMessage>();

        public SubscriptionClient(AccountsTokenClient tokenClient, AltaClientConfiguration clientConfiguration, ILogger<SubscriptionClient> logger)
        {
            this.tokenClient = tokenClient;
            this.clientConfiguration = clientConfiguration;
            this.logger = logger;
            this.migrationTaskSource.SetResult();
        }

        //NOTE: consider capturing operation contexts functionally to prevent state mangling

        public async Task Connect()
        {
            if (this.state != State.Disconnected)
            {
                throw new InvalidOperationException("Connect cannot be called on an already connected client.");
            }

            // State must be disconnected then, but let's make sure only one creator at a time.  Use the semaphore.
            if (this.state == State.Disconnected)
            {
                await this.creatingWebsocket.WaitAsync();

                try
                {
                    // Check the state again.
                    if (this.state == State.Disconnected)
                    {
                        var client = await CreateWebsocketClient();
                        RegisterSubscriptionsForWebsocket(client);
                        this.client = client;
                        this.state = State.Connected;
                        ScheduleMigration();
                    }
                }
                finally
                {
                    this.creatingWebsocket.Release();
                }
            }
        }

        public async Task Subscribe(string eventname, string key)
        {
            if (this.state == State.Disconnected)
            {
                throw new InvalidOperationException("Subscribe cannot be called on a disconnected client.");
            }

            // make sure we aren't migrating
            await migrationTaskSource.Task;

            if (!this.subscriptions.ContainsKey(eventname))
            {
                this.subscriptions.TryAdd(eventname, new ConcurrentBag<string>());
            }

            if (!this.subscriptions[eventname].Contains(key))
            {
                await this.SendSubscriptionRequest(this.client!, eventname, key);
                this.subscriptions[eventname].Add(key);
            }
        }

        private async Task SendSubscriptionRequest(WebsocketClient client, string eventname, string key)
        {
            await this.SendMessageWithWebSocketClient(client, HttpMethod.Post, $"subscription/{eventname}/{key}");
        }

        // Websocket Implementation
        private async Task<WebsocketClient> CreateWebsocketClient()
        {
            logger.LogInformation("Creating a new websocket client");

            var client = new WebsocketClient(SubscriptionWebsocketUri, WebSocketFactory)
            {
                Name = $"Subscriptions-{DateTime.UtcNow}",
                ReconnectTimeout = null,
                ErrorReconnectTimeout = TimeSpan.Zero
            };

            await client.Start();
            this.state = State.Connected;
            return client;
        }

        private ClientWebSocket WebSocketFactory()
        {
            this.authToken = this.tokenClient.GetValidToken().Result.AccessToken;
            var client = new ClientWebSocket();
            client.Options.KeepAliveInterval = TimeSpan.FromMinutes(15);
            client.Options.SetRequestHeader("Authorization", $"Bearer {this.authToken}");
            client.Options.SetRequestHeader("x-api-key", clientConfiguration.ApiKey);
            client.Options.SetRequestHeader("Content-Type", "application/json");
            return client;
        }

        private async Task<string> SendMessage(HttpMethod method, string path)
        {
            if (this.state == State.Disconnected)
            {
                throw new InvalidOperationException("Unable to send messages on a disconnected client.");
            }

            return await SendMessageWithWebSocketClient(this.client!, method, path);
        }

        private async Task<string> SendMessageWithWebSocketClient(WebsocketClient client, HttpMethod method, string path, object? payload = default)
        {
            // make sure we aren't migrating (unless path is migrate)
            if (path != "migrate")
            {
                await migrationTaskSource.Task;
            }

            var id = this.messageId++;
            var message = new SubscriptionClientRequestMessage(id, method, path, this.authToken!, payload);
            this.logger.LogDebug(message.ToString());

            // Prepare to receive single response.
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();

            client
                .MessageReceived
                .Where(message => IsResponseMessageForId(message, id))
                .Take(1)
                .Timeout(ResponseTimeout)
                .Subscribe(response =>
                {
                    this.logger.LogDebug($"Got expected response for id: {id}.{Environment.NewLine}{response.Text}");
                    tcs.SetResult(response.Text);
                },
                exception =>
                {
                    this.logger.LogError(exception, $"An error occurred while awaiting response for id: {id}");
                    tcs.SetException(exception);
                });

            var messageText = JsonSerializer.Serialize(message);
            client.Send(messageText);

            return await tcs.Task;
        }

        private void ScheduleMigration()
        {
            Task.Delay(TimeSpan.FromMinutes(1)).ContinueWith(_ => MigrateWebSocket());
        }

        private async Task MigrateWebSocket()
        {
            if (this.state == State.Connected)
            {
                WebsocketClient oldClient = this.client!;
                WebsocketClient newClient = await CreateWebsocketClient();
                this.RegisterSubscriptionsForWebsocket(newClient);
                try
                {
                    // only one thing should be calling migrate at a time at all since there's only the one migration callsite.
                    this.StartMigration();
                    var token = await GetMigrationToken();

                    // send migration token on a new websocket!
                    string migrationResult = await this.SendMessageWithWebSocketClient(newClient, HttpMethod.Post, "migrate", new { token = token });
                    logger.LogInformation(migrationResult);

                    if (migrationResult.Contains("error"))
                    {
                        throw new SubscriptionClientMigrationFailedException($"An error occurred while attempting to migrate subscriptions to a new WebSocket connection. The Alta API responded with a error message.{Environment.NewLine}{migrationResult}");
                    }
                }
                catch (Exception)
                {
                    await RecoverSubscriptions(newClient);
                }

                //Success! Let's close the old websocket and swap out for the new one.
                this.client = newClient;
                await this.CloseWebsocket(oldClient);
                CompleteMigration();
                this.ScheduleMigration();
            }
        }

        private void StartMigration()
        {
            this.state = State.Migrating;
            logger.LogWarning($"MIGRATION STARTED! {DateTime.Now}");
            this.migrationTaskSource = new TaskCompletionSource();
        }

        private void CompleteMigration()
        {
            logger.LogWarning($"MIGRATION COMPLETED! {DateTime.Now}");
            this.state = State.Connected;
            this.migrationTaskSource.SetResult();            
        }

        private async Task RecoverSubscriptions(WebsocketClient recoveryClient)
        {
            if (this.state == State.Migrating)
            {
                logger.LogWarning($"ATTEMPTING RECOVERY! {DateTime.Now}");
                foreach (var eventName in this.subscriptions.Keys)
                {
                    await Parallel.ForEachAsync(this.subscriptions[eventName], new ParallelOptions { MaxDegreeOfParallelism = 10 }, async (key, _) =>
                    {
                        try
                        {
                            await this.SendSubscriptionRequest(recoveryClient, eventName, key);
                        }
                        catch (TimeoutException timeout)
                        {
                            logger.LogError(timeout, $"Timeout occurred during subscription recovery for {eventName}/{key}");
                        }
                    });
                }
            }
        }

        private async Task<string> GetMigrationToken()
        {
            string? migrationTokenRequestResult = null;
            try
            {
                migrationTokenRequestResult = await this.SendMessage(HttpMethod.Get, "migrate");
                logger.LogInformation(migrationTokenRequestResult);
                var response = JsonSerializer.Deserialize<SubscriptionClientMigrationTokenMessage>(migrationTokenRequestResult, WebApiJsonSerializerOptions.Default)!;
                return response.Content.Token;
            }
            catch (TimeoutException timeout)
            {
                logger.LogError("Timeout occurred while awaiting response to migration request.", timeout);
                throw;
            }
            catch (NotSupportedException nse)
            {
                logger.LogError($"Unable to serialize the response from the migration request.  This likely means the response does not indicated success.{Environment.NewLine}{migrationTokenRequestResult??"N/A"}", nse);
                throw;
            }
        }

        private void RegisterSubscriptionsForWebsocket(WebsocketClient newClient)
        {
            var groupedMessages = newClient.MessageReceived.GroupBy(ExtractEventType);

            groupedMessages.Where(grp => grp.Key == "group-server-status")
                .SelectMany(grp => grp.Select(TransformGroupServerStatus<GroupServerStatusMessage>))
                .Subscribe(message => this.GroupStatusChangedSubject.OnNext(message));
        }

        private async Task CloseWebsocket(WebsocketClient client)
        {
            await client.Stop(WebSocketCloseStatus.NormalClosure, "WebSocket closed by client.");
            client.NativeClient.Dispose();
            client.Dispose();
        }

        // Message Subscription Management
        private bool IsResponseMessageForId(ResponseMessage message, int id)
        {
            var jsonMessage = JsonDocument.Parse(message.Text);
            if (jsonMessage.RootElement.TryGetProperty("event", out var @event))
            {
                var eventValue = @event.GetString();
                if (eventValue != "response")
                {
                    return false;
                }
            }

            if (jsonMessage.RootElement.TryGetProperty("id", out var messageId))
            {
                return id == messageId.GetInt32();
            }

            return false;
        }

        private TMessage TransformGroupServerStatus<TMessage>(ResponseMessage message)
        {
            return JsonSerializer.Deserialize<TMessage>(message.Text, WebApiJsonSerializerOptions.Default)!;
        }

        private string ExtractEventType(ResponseMessage message)
        {
            var jsonMessage = JsonDocument.Parse(message.Text);
            if (jsonMessage.RootElement.TryGetProperty("event", out var @event))
            {
                return @event.GetString()!;
            }

            return "";
        }

        // Message Subscription Handlers
        private void MessageReceived(ResponseMessage message)
        {
            this.logger.LogDebug($"RECEIVED:{message.Text}");
        }

        private void DisconnectionHappened(DisconnectionInfo message)
        {
            this.logger.LogWarning($"DISCONNECTED: {message.CloseStatus} :: {message.CloseStatusDescription}");
        }

        private void ReconnectionHappened(ReconnectionInfo message)
        {
            this.logger.LogWarning($"RECONNECTED: {message.Type}");
        }

        // Disposal
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.client?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        //private async Task EnsureWebsocketConnected()
        //{
        //    if (this.state == State.Disconnected)
        //    {
        //        await this.creatingWebsocket.WaitAsync();

        //        try
        //        {
        //            // Check the state again.
        //            if (this.state == State.Disconnected)
        //            {
        //                logger.LogInformation("Creating a new websocket client");

        //                // Cancel any existing websocket.
        //                this.cancellationTokenSource.Cancel();
        //                this.cancellationTokenSource = new CancellationTokenSource();

        //                var client = new WebsocketClient(SubscriptionWebsocketUri, WebSocketFactory)
        //                {
        //                    Name = $"Subscriptions-{DateTime.UtcNow}",
        //                    ReconnectTimeout = null,
        //                    ErrorReconnectTimeout = TimeSpan.Zero
        //                };

        //                //var messageReceivedSubscription = client.MessageReceived.Subscribe(MessageReceived);
        //                //var disconnectionHappenedSubscription = client.DisconnectionHappened.Subscribe(DisconnectionHappened);
        //                //var reconnectionHappenedSubscription = client.ReconnectionHappened.Subscribe(ReconnectionHappened);

        //                //var groupedMessages = client.MessageReceived.GroupBy(ExtractEventType);

        //                //groupedMessages.Where(grp => grp.Key == "group-server-status")
        //                //    .SelectMany(grp => grp.Select(TransformGroupServerStatus<GroupServerStatusMessage>))
        //                //    .Subscribe(message => this.GroupStatusChanged.OnNext(message));

        //                await client.Start();
        //                this.cancellationTokenSource.Token.Register(async () =>
        //                {
        //                    await CloseWebsocket(client);
        //                    //messageReceivedSubscription.Dispose();
        //                    //disconnectionHappenedSubscription.Dispose();
        //                    //reconnectionHappenedSubscription.Dispose();
        //                });

        //                this.client = client;
        //                this.state = State.Connected;
        //                this.migrationDue = Task.Delay(TimeSpan.FromMinutes(1)).ContinueWith(_ => MigrateWebSocket());
        //            }
        //        }
        //        finally
        //        {
        //            this.creatingWebsocket.Release();
        //        }
        //    }
        //}

        // Internal Types
        private record SubscriptionClientRequestMessage
        {
            private readonly bool obfuscateContent;

            public SubscriptionClientRequestMessage(int id, HttpMethod method, string path, string token, object? content = default, bool obfuscatePayload = true)
            {
                Id = id;
                Method = method.Method;
                Path = path;
                Authorization = $"Bearer {token}";
                Content = content == null ? content : JsonSerializer.Serialize(content, WebApiJsonSerializerOptions.Default);
                this.obfuscateContent = obfuscatePayload;
            }

            public int Id { get; init; }

            public string Method { get; init; }

            public string Path { get; init; }

            public string Authorization { get; init; }

            public object? Content { get; init; }

            public override string ToString()
            {
                var output = $"id: {Id} method: {Method} path: {Path}";

                if (Content != null && obfuscateContent != true)
                {
                    output += $" content: {Content}";
                }

                return output;
            }
        }

        private record SubscriptionClientMigrationTokenMessage
        {
            public int Id { get; set; }

            public string Event { get; set; }

            public string Key { get; set; }

            public int ResponseCode { get; set; }

            [JsonConverter(typeof(EmbeddedJsonConverter<MigrationTokenContent>))]
            public MigrationTokenContent Content { get; set; }

            public override string ToString() => $"id: {Id} event: {Event} key: {Key} responseCode: {ResponseCode}";

            public record MigrationTokenContent
            {
                public string Token { get; set; }
            }
        }

        private enum State
        {
            Disconnected,
            Connected,
            Migrating
        }
    }
}
