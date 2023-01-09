﻿//using Microsoft.Extensions.Logging;
//using Polly;
//using System.Collections.Concurrent;
//using System.Net.WebSockets;
//using System.Reactive.Linq;
//using System.Reactive.Subjects;
//using System.Text.Json;
//using static Townsharp.Api.Json.JsonUtils;
//using Townsharp.Infra.Alta.Configuration;
//using Websocket.Client;
//using Websocket.Client.Models;
//using Townsharp.Infra.Alta.Identity;
//using Townsharp.Servers;
//using Townsharp.Groups;
//using Townsharp.Infra.Alta.Subscriptions;

//namespace Townsharp.Subscriptions
//{
//    public class SubscriptionClient : IDisposable
//    {
//        public readonly static Uri SubscriptionWebsocketUri = new Uri("wss://5wx2mgoj95.execute-api.ap-southeast-2.amazonaws.com/dev");
//        public readonly static TimeSpan ResponseTimeout = TimeSpan.FromSeconds(15);
//        public readonly static TimeSpan MigrationFrequency = TimeSpan.FromMinutes(20);

//        public readonly AsyncPolicy<string> messageRetryPolicy;
//        public readonly AsyncPolicy recoveryPolicy;

//        // Dependencies
//        private readonly AccountsTokenClient tokenClient;
//        private readonly AltaClientConfiguration clientConfiguration;
//        private readonly ILogger<SubscriptionClient> logger;

//        // WebSocket state
//        private State state = State.Disconnected;
//        private WebsocketClient? client;
//        private bool disposedValue = false;
//        private int messageId = 1;
//        private string? authToken;
//        private ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> subscriptions = new ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>();

//        // Scheduling and synchronization
//        private SemaphoreSlim creatingWebsocket = new SemaphoreSlim(1, 1);
//        private TaskCompletionSource migrationTaskSource = new TaskCompletionSource();
//        private CancellationTokenSource scheduledMigrationCancellationTokenSource = new CancellationTokenSource();

//        // Subscription Handlers
//        // NOTE: make this public and subscribe on generics with a handler method.
//        // Alternatively, may not be an appropriate type to even be emitting domain models, time to design the domain interface.
//        private readonly Subject<ServerStatusMessage> ServerStatusChangedSubject = new Subject<ServerStatusMessage>();

//        public IObservable<ServerStatusChangedEvent> ServerStatusChanged => ServerStatusChangedSubject.Select(TransformServerStatusChangedEvent);

//        private ServerStatusChangedEvent TransformServerStatusChangedEvent(ServerStatusMessage message)
//        {
//            return new ServerStatusChangedEvent(
//                new ServerId(message.Content.Id),
//                new GroupId(message.Content.GroupId),
//                message.Content.IsOnline,
//                message.Content.OnlinePlayers.Select(player => new PlayerInfo(new PlayerId(player.Id), player.Username)).ToArray());
//        }

//        // NOTE: Consider switching to att-client's "disconnected/reconnected" model and push resubscription upstream into the application model.
//        // !!!!!!!!!!!!!! IN FACT! THIS HELPS TOWNSHARP LOGIC TO "RESYNCHRONIZE" ASSUMED STATE FOR THINGS LIKE SERVER POPULATIONS. THIS IS HELPFUL! !!!!!!!!!!!!!!!!
//        public SubscriptionClient(AccountsTokenClient tokenClient, AltaClientConfiguration clientConfiguration, ILogger<SubscriptionClient> logger)
//        {
//            this.tokenClient = tokenClient;
//            this.clientConfiguration = clientConfiguration;
//            this.logger = logger;

//            // needed so that awaiters will immediately proceed.  We aren't migrating at the start, so there's nothing to wait for.
//            migrationTaskSource.SetResult();

//            // Setup our policies
//            messageRetryPolicy = Policy<string>.Handle<TimeoutException>()
//                .RetryAsync(2);

//            recoveryPolicy = Policy.Handle<Exception>()
//                .WaitAndRetryForeverAsync(
//                    i => TimeSpan.FromSeconds(Math.Max(i, 5)),
//                    (exc, i, time) => this.logger.LogInformation($"Attempting recovery retry. ({i}) - {time.TotalSeconds}s delay - last error: {exc.Message}"));
//        }

//        // NOTE: consider capturing operation contexts functionally to prevent state mangling
//        // (meta note, I'm not sure what I meant here, probably applies to a different method than Connect.)
//        // I think I'm trying to say we need a "run with context" kind of mechanism.  I don't think we do.  Probably delete these comments in cleanup.
//        // We -may- however be able to get away from Connect being needed...
//        public async Task Connect()
//        {
//            if (state != State.Disconnected)
//            {
//                throw new InvalidOperationException("Connect cannot be called on an already connected client.");
//            }

//            // State must be disconnected then, but let's make sure only one creator at a time.  Use the semaphore.
//            if (state == State.Disconnected)
//            {
//                await creatingWebsocket.WaitAsync();

//                try
//                {
//                    // Check the state again.
//                    if (state == State.Disconnected)
//                    {
//                        var client = await CreateWebsocketClient();
//                        this.RegisterSubscriptionsForWebsocket(client);
//                        this.client = client;
//                        state = State.Connected;
//                        ScheduleMigration();
//                    }
//                }
//                finally
//                {
//                    creatingWebsocket.Release();
//                }
//            }
//        }

//        public async Task Subscribe(string eventname, string key)
//        {
//            if (state == State.Disconnected)
//            {
//                throw new InvalidOperationException("Subscribe cannot be called on a disconnected client.");
//            }

//            // make sure we aren't migrating
//            await migrationTaskSource.Task;

//            if (!subscriptions.ContainsKey(eventname))
//            {
//                subscriptions.TryAdd(eventname, new ConcurrentDictionary<string, bool>());
//            }

//            if (!subscriptions[eventname].ContainsKey(key))
//            {
//                if (subscriptions[eventname].TryAdd(key, true))
//                {
//                    await this.SendSubscriptionRequest(client!, eventname, key);
//                }
//            }
//        }

//        public async Task<bool> Unsubscribe(string eventname, string key)
//        {
//            if (state == State.Disconnected)
//            {
//                throw new InvalidOperationException("Unsubscribe cannot be called on a disconnected client.");
//            }

//            // make sure we aren't migrating
//            await migrationTaskSource.Task;

//            if (!subscriptions.ContainsKey(eventname))
//            {
//                // Then we can't unsubscribe
//                // Silent fail?  Exit?  Return a descriptive type?
//                return false;
//            }

//            if (!subscriptions[eventname].ContainsKey(key))
//            {
//                // Then we can't unsubscribe
//                // Silent fail?  Exit?  Return a descriptive type?
//                return false;
//            }

//            if (subscriptions[eventname].Remove(key, out bool _))
//            {
//                await this.SendUnscriptionRequest(client!, eventname, key);
//                return true;
//            }

//            return false;
//        }

//        private async Task SendSubscriptionRequest(WebsocketClient client, string eventname, string key)
//        {
//            await messageRetryPolicy.ExecuteAsync(async () => await SendMessage(client, HttpMethod.Post, $"subscription/{eventname}/{key}"));
//        }

//        private async Task SendUnscriptionRequest(WebsocketClient client, string eventname, string key)
//        {
//            await messageRetryPolicy.ExecuteAsync(async () => await SendMessage(client, HttpMethod.Delete, $"subscription/{eventname}/{key}"));
//        }

//        // Websocket Implementation
//        private async Task<WebsocketClient> CreateWebsocketClient()
//        {
//            logger.LogDebug("Creating a new websocket client");

//            var client = new WebsocketClient(SubscriptionWebsocketUri, () =>
//                {
//                    authToken = tokenClient.GetValidToken().Result.AccessToken;
//                    var client = new ClientWebSocket();
//                    client.Options.KeepAliveInterval = TimeSpan.FromMinutes(15);
//                    client.Options.SetRequestHeader("Authorization", $"Bearer {authToken}");
//                    client.Options.SetRequestHeader("x-api-key", clientConfiguration.ApiKey); // NOPE
//                    client.Options.SetRequestHeader("Content-Type", "application/json");
//                    return client;
//                })
//            {
//                Name = $"Subscriptions-{DateTime.UtcNow}",
//                ReconnectTimeout = null,
//                ErrorReconnectTimeout = TimeSpan.Zero
//            };

//            client.MessageReceived.Subscribe(MessageReceived);
//            client.DisconnectionHappened.Subscribe(DisconnectionHappened);
//            client.ReconnectionHappened.Subscribe(ReconnectionHappened);

//            await client.Start();
//            state = State.Connected;
//            return client;
//        }

//        private async Task<string> SendMessage(WebsocketClient client, HttpMethod method, string path, string content = "")
//        {
//            if (state == State.Disconnected)
//            {
//                throw new InvalidOperationException("Unable to send messages on a disconnected client.");
//            }

//            // Delay outgoing messages (unless path is migrate)
//            if (path != "migrate")
//            {
//                await migrationTaskSource.Task;
//            }

//            var id = messageId++;
//            SubscriptionRequestMessage message = new SubscriptionRequestMessage(id, method, path, authToken!, content);
//            logger.LogDebug(message.ToString());

//            // Prepare to receive single response.
//            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();

//            client
//                .MessageReceived
//                .Where(response => IsResponseMessageForId(response, id))
//                .Take(1)
//                .Timeout(ResponseTimeout)
//                .Subscribe(response =>
//                {
//                    logger.LogDebug($"Got expected response for id: {id}.{Environment.NewLine}{response.Text}");
//                    tcs.SetResult(response.Text);
//                },
//                exception =>
//                {
//                    logger.LogWarning(exception, $"An error occurred while awaiting response for id: {id}");
//                    tcs.SetException(exception);
//                });

//            var messageText = Serialize(message);
//            client.Send(messageText);

//            return await tcs.Task;
//        }

//        private void ScheduleMigration()
//        {
//            scheduledMigrationCancellationTokenSource.Cancel();
//            scheduledMigrationCancellationTokenSource = new CancellationTokenSource();
//            Task.Delay(MigrationFrequency, scheduledMigrationCancellationTokenSource.Token).ContinueWith(_ => MigrateWebSocket(), scheduledMigrationCancellationTokenSource.Token);
//        }

//        private async Task MigrateWebSocket()
//        {
//            if (state == State.Connected)
//            {
//                WebsocketClient oldClient = client!;
//                WebsocketClient newClient = await CreateWebsocketClient();
//                RegisterSubscriptionsForWebsocket(newClient);
//                try
//                {
//                    // only one thing should be calling migrate at a time at all since there's only the one migration callsite.
//                    state = State.Migrating;
//                    logger.LogWarning($"MIGRATION STARTED! {DateTime.Now}");
//                    migrationTaskSource = new TaskCompletionSource();
//                    var token = await GetMigrationToken(oldClient);

//                    // send migration token on a new websocket!
//                    string migrationResult = await messageRetryPolicy.ExecuteAsync(async () => await SendMessage(newClient, HttpMethod.Post, "migrate", Serialize(new { token })));
//                    logger.LogDebug($"MigrationResult {migrationResult}");

//                    // Note: NOPE! Serialize the response if it's JSON and check for errors.  If there's no json, it's an error anyhow.  But this matches on usernames and random BS.
//                    if (migrationResult.Contains("error"))
//                    {
//                        throw new SubscriptionClientMigrationFailedException($"An error occurred while attempting to migrate subscriptions to a new WebSocket connection. The Alta API responded with a error message.{Environment.NewLine}{migrationResult}");
//                    }
//                }
//                catch (Exception)
//                {
//                    await RecoverSubscriptions(newClient);
//                }

//                //Success! Let's close the old websocket and swap out for the new one.
//                client = newClient;
//                await CloseWebsocket(oldClient);
//                logger.LogWarning($"MIGRATION COMPLETED! {DateTime.Now}");
//                state = State.Connected;
//                migrationTaskSource.SetResult();
//                ScheduleMigration();
//            }
//        }

//        private async Task RecoverSubscriptions(WebsocketClient recoveryClient)
//        {
//            if (state == State.Migrating)
//            {
//                logger.LogWarning($"ATTEMPTING RECOVERY FROM FAILED MIGRATION! {DateTime.Now}");
//            }

//            if (state == State.Faulted)
//            {
//                logger.LogWarning($"ATTEMPTING RECOVERY FROM UNEXPECTEDLY TERMINATED CONNECTION! {DateTime.Now}");
//            }

//            await recoveryPolicy.ExecuteAsync(async () =>
//                {
//                    foreach (var eventName in subscriptions.Keys)
//                    {
//                        await Parallel.ForEachAsync(subscriptions[eventName].Keys, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async (key, _) =>
//                        {
//                            try
//                            {
//                                await SendSubscriptionRequest(recoveryClient, eventName, key);
//                            }
//                            catch (TimeoutException timeout)
//                            {
//                                logger.LogError(timeout, $"Timeout occurred during subscription recovery for {eventName}/{key}");
//                            }
//                        });
//                    }

//                    state = State.Connected;
//                });
//        }

//        private async Task RecoverFaultedConnection()
//        {
//            state = State.Faulted;
//            logger.LogWarning("The connection with the server was lost.  Assuming all subscriptions were wiped out.  Attempting recovery.");
//            WebsocketClient oldClient = client!;
//            WebsocketClient newClient = await CreateWebsocketClient();
//            RegisterSubscriptionsForWebsocket(newClient);
//            ScheduleMigration();
//            await RecoverSubscriptions(newClient);
//            client = newClient;
//            await CloseWebsocket(oldClient);
//            logger.LogWarning("Connection re-established and subscriptions restored.");
//            state = State.Connected;
//        }

//        private async Task<string> GetMigrationToken(WebsocketClient client)
//        {
//            string? migrationTokenRequestResult = null;
//            try
//            {
//                // no retry here, once it's sent we have to assume migration was accepted, if we don't get a reply, we have to execute a recovery.
//                migrationTokenRequestResult = await SendMessage(client, HttpMethod.Get, "migrate");
//                logger.LogDebug($"Migration Request Result {migrationTokenRequestResult}");

//                var response = Deserialize<SubscriptionResponseMessage>(migrationTokenRequestResult);

//                if (response.Content.Contains("token", StringComparison.InvariantCultureIgnoreCase))
//                {
//                    var content = Deserialize<MigrationTokenContent>(response.Content);

//                    return content.Token;
//                }
//                else if (response.Content.Contains("error_code", StringComparison.InvariantCultureIgnoreCase))
//                {
//                    var content = Deserialize<ErrorContent>(response.Content);
//                    logger.LogError($"An error occurred during the migration request {content.ErrorCode} - {content.Message}");

//                    throw new AltaSubscriptionClientException(content.Message, content.ErrorCode);
//                }
//                else
//                {
//                    throw new InvalidOperationException($"An unexpected error occured during migration.");
//                }
//            }
//            catch (TimeoutException timeout)
//            {
//                logger.LogError("Timeout occurred while awaiting response to migration request.", timeout);
//                throw;
//            }
//            catch (NotSupportedException nse)
//            {
//                logger.LogError($"Unable to serialize the response from the migration request.  This likely means the response does not indicated success.{Environment.NewLine}{migrationTokenRequestResult ?? "N/A"}", nse);
//                throw;
//            }
//        }

//        private void RegisterSubscriptionsForWebsocket(WebsocketClient client)
//        {
//            logger.LogDebug($"Registering subscriptions for websocket. {client.Name}");
//            var groupedMessages = client.MessageReceived.GroupBy(ExtractEventType);

//            groupedMessages.Where(grp => grp.Key == "group-server-status")
//                .SelectMany(grp => grp.Select(TransformGroupServerStatus<ServerStatusMessage>))
//                .Subscribe(message => ServerStatusChangedSubject.OnNext(message));

//            client
//                .MessageReceived
//                .Where(response => response.Text.Contains("Error", StringComparison.InvariantCultureIgnoreCase))
//                .Subscribe(response =>
//                {
//                    logger.LogError($"A response was received with an error.{Environment.NewLine}{response.Text}");
//                });

//            client
//                .MessageReceived
//                .Where(response => response.Text.Contains("\"responseCode\":", StringComparison.InvariantCultureIgnoreCase) && !response.Text.Contains("\"responseCode\":200", StringComparison.InvariantCultureIgnoreCase))
//                .Subscribe(response =>
//                {
//                    logger.LogError($"A response was received with a status code that does not indicate success.{Environment.NewLine}{response.Text}");
//                });
//        }

//        private async Task CloseWebsocket(WebsocketClient client)
//        {
//            await client.Stop(WebSocketCloseStatus.NormalClosure, "WebSocket closed by client.");
//            client.NativeClient.Dispose();
//            client.Dispose();
//        }

//        // Message Subscription Management
//        private bool IsResponseMessageForId(ResponseMessage message, int id)
//        {
//            // NOTE: serialize this...
//            var jsonMessage = JsonDocument.Parse(message.Text);
//            if (jsonMessage.RootElement.TryGetProperty("event", out var @event))
//            {
//                var eventValue = @event.GetString();
//                if (eventValue != "response")
//                {
//                    return false;
//                }
//            }

//            if (jsonMessage.RootElement.TryGetProperty("id", out var messageId))
//            {
//                return id == messageId.GetInt32();
//            }

//            return false;
//        }

//        private TMessage TransformGroupServerStatus<TMessage>(ResponseMessage message)
//        {
//            return Deserialize<TMessage>(message.Text)!;
//        }

//        private string ExtractEventType(ResponseMessage message)
//        {
//            var jsonMessage = JsonDocument.Parse(message.Text);
//            if (jsonMessage.RootElement.TryGetProperty("event", out var @event))
//            {
//                return @event.GetString()!;
//            }

//            return "";
//        }

//        private void MessageReceived(ResponseMessage responseMessage)
//        {
//            logger.LogDebug($"RECEIVED: {responseMessage.Text}");
//        }

//        private void DisconnectionHappened(DisconnectionInfo disconnectionInfo)
//        {
//            logger.LogDebug($"DISCONNECTED: Type-{disconnectionInfo.Type} Status-{disconnectionInfo.CloseStatus} :: {disconnectionInfo.CloseStatusDescription}");

//            if (disconnectionInfo.Type == DisconnectionType.Lost)
//            {
//                // we got dropped.  Something happend Alta side, perhaps a service update.  Either way, we have to assume all of our subscriptions were lost.  Let's initiate recovery.
//                Task.Run(RecoverFaultedConnection);
//            }
//        }

//        private void ReconnectionHappened(ReconnectionInfo reconnectionInfo)
//        {
//            logger.LogDebug($"RECONNECTED: Type-{reconnectionInfo.Type}");
//        }

//        // Disposal
//        protected virtual void Dispose(bool disposing)
//        {
//            if (!disposedValue)
//            {
//                if (disposing)
//                {
//                    client?.Dispose();
//                }

//                disposedValue = true;
//            }
//        }

//        public void Dispose()
//        {
//            Dispose(disposing: true);
//            GC.SuppressFinalize(this);
//        }

//        // Internal Types
//        private record SubscriptionRequestMessage
//        {
//            private readonly bool obfuscateContent;

//            public SubscriptionRequestMessage(int id, HttpMethod method, string path, string token, string content = "", bool obfuscateContent = true)
//            {
//                Id = id;
//                Method = method.Method;
//                Path = path;
//                Authorization = $"Bearer {token}";
//                Content = content;
//                this.obfuscateContent = obfuscateContent;
//            }

//            public int Id { get; init; }

//            public string Method { get; init; }

//            public string Path { get; init; }

//            public string Authorization { get; init; }

//            public string Content { get; init; }

//            public override string ToString()
//            {
//                var output = $"id: {Id} method: {Method} path: {Path}";

//                if (Content != null && obfuscateContent != true)
//                {
//                    output += $" content: {Content}";
//                }

//                return output;
//            }
//        }

//        private record SubscriptionResponseMessage
//        {
//            public int Id { get; init; }

//            public string Event { get; init; } = string.Empty;

//            public string Key { get; init; } = string.Empty;

//            public int ResponseCode { get; init; }

//            public string Content { get; init; } = string.Empty;

//            public override string ToString() => $"id: {Id} event: {Event} key: {Key} responseCode: {ResponseCode}";
//        }

//        private record MigrationTokenContent
//        {
//            public string Token { get; init; } = string.Empty;
//        }

//        private record ErrorContent
//        {
//            public string Message { get; init; } = string.Empty;

//            public string ErrorCode { get; init; } = string.Empty;
//        }

//        private enum State
//        {
//            Disconnected,
//            Connected,
//            Migrating,
//            Faulted
//        }
//    }
//}