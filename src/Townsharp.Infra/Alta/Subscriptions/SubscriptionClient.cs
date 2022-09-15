using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using Townsharp.Infra.Alta.Api;
using Townsharp.Infra.Alta.Api.Identity;
using Townsharp.Infra.Alta.Configuration;
using Websocket.Client;
using Websocket.Client.Models;

namespace Townsharp.Infra.Alta.Subscriptions
{
    public class SubscriptionClient : IDisposable
    {
        public readonly static Uri SubscriptionWebsocketUri = new Uri("wss://5wx2mgoj95.execute-api.ap-southeast-2.amazonaws.com/dev");

        private readonly AccountsTokenClient tokenClient;
        private readonly AltaClientConfiguration clientConfiguration;
        private readonly ILogger<SubscriptionClient> logger;
        private readonly object syncronization = new object();
        private readonly JsonSerializerOptions serializerOptions;

        private bool disposedValue;
        private int messageId = 1;
        private State state = State.Disconnected;
        private WebsocketClient? client;
        private string? token;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private Task migrationTask = Task.CompletedTask;
        private TimeSpan responseTimeout = TimeSpan.FromSeconds(15);
        private SemaphoreSlim creatingWebsocket = new SemaphoreSlim(1, 1);

        public int sendTotal = 0;
        public int receiveTotal = 0;
        public int errorTotal = 0;
        public int totalMessages = 0;

        // NOTE: make this public and subscribe on generics with a handler method.
        public readonly Subject<GroupServerStatusMessage> GroupStatusChanged;

        public SubscriptionClient(AccountsTokenClient tokenClient, AltaClientConfiguration clientConfiguration, ILogger<SubscriptionClient> logger)
        {
            this.tokenClient = tokenClient;
            this.clientConfiguration = clientConfiguration;
            this.logger = logger;
            this.GroupStatusChanged = new Subject<GroupServerStatusMessage>();
            this.serializerOptions = WebApiJsonSerializerOptions.Default;
        }

        public async Task SendMessage(HttpMethod method, string path)
        {
            await this.EnsureWebsocketConnected();

            var id = this.messageId++;
            var message = new SubscriptionClientRequestMessage(id, method, path, this.token!);
            this.logger.LogDebug(message.ToString());

            var client = this.client!;
            sendTotal++;

            // Prepare to receive single response.
            TaskCompletionSource tcs = new TaskCompletionSource();

            client
                .MessageReceived
                .Where(message => IsResponseMessageForId(message, id))
                .Take(1)
                .Timeout(this.responseTimeout)
                .Subscribe(response => 
                {
                    receiveTotal++;
                    this.logger.LogDebug($"Got expected response for id: {id}.{Environment.NewLine}{response}");
                },
                exception =>
                {
                    errorTotal++;
                    this.logger.LogError(exception, $"An error occurred while awaiting response for id: {id}");
                    tcs.SetException(exception);
                },
                () => 
                { 
                    tcs.SetResult();
                });

            client.Send(JsonSerializer.Serialize(message));

            await tcs.Task;
        }

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

        private async Task EnsureWebsocketConnected()
        {
            if (this.state == State.Disconnected)
            {
                await this.creatingWebsocket.WaitAsync();

                try
                {
                    // Check the state again.
                    if (this.state == State.Disconnected)
                    {
                        logger.LogInformation("Creating a new websocket client");

                        // Cancel any existing websocket.
                        cancellationTokenSource.Cancel();
                        cancellationTokenSource = new CancellationTokenSource();

                        var client = new WebsocketClient(SubscriptionWebsocketUri, WebSocketFactory)
                        {
                            Name = $"Subscriptions-{DateTime.UtcNow}",
                            ReconnectTimeout = null,
                            ErrorReconnectTimeout = TimeSpan.Zero
                        };

                        var messageReceivedSubscription = client.MessageReceived.Subscribe(MessageReceived);
                        var disconnectionHappenedSubscription = client.DisconnectionHappened.Subscribe(DisconnectionHappened);
                        var reconnectionHappenedSubscription = client.ReconnectionHappened.Subscribe(ReconnectionHappened);

                        var groupedMessages = client.MessageReceived.GroupBy(ExtractEventType);

                        groupedMessages.Where(grp => grp.Key == "group-server-status")
                            .SelectMany(grp => grp.Select(TransformGroupServerStatus<GroupServerStatusMessage>))
                            .Subscribe(message => this.GroupStatusChanged.OnNext(message));

                        await client.Start();
                        cancellationTokenSource.Token.Register(async () =>
                        {
                            await CloseWebsocket(client);
                            messageReceivedSubscription.Dispose();
                            disconnectionHappenedSubscription.Dispose();
                            reconnectionHappenedSubscription.Dispose();
                        });

                        this.client = client;
                        this.state = State.Connected;
                    }
                }
                finally
                {
                    this.creatingWebsocket.Release();
                }
            }
        }

        private TMessage TransformGroupServerStatus<TMessage>(ResponseMessage message)
        {
            return JsonSerializer.Deserialize<TMessage>(message.Text, this.serializerOptions)!;
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

        private void MessageReceived(ResponseMessage message)
        {
            this.totalMessages++;
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

        private ClientWebSocket WebSocketFactory()
        {
            this.token = this.tokenClient.GetValidToken().Result.AccessToken;
            var client = new ClientWebSocket();
            client.Options.KeepAliveInterval = TimeSpan.FromMinutes(15);
            client.Options.SetRequestHeader("Authorization", $"Bearer {this.token}");
            client.Options.SetRequestHeader("x-api-key", clientConfiguration.ApiKey);
            client.Options.SetRequestHeader("Content-Type", "application/json");
            return client;
        }

        private async Task CloseWebsocket(WebsocketClient client)
        {
            await client.Stop(WebSocketCloseStatus.NormalClosure, "WebSocket closed by client.");
            client.NativeClient.Dispose();
        }

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

        private record SubscriptionClientRequestMessage
        {
            public SubscriptionClientRequestMessage(int id, HttpMethod method, string path, string token)
            {
                this.Id = id;
                this.Method = method.Method;
                this.Path = path;
                this.Authorization = $"Bearer {token}";
            }

            public int Id { get; init; } 
            
            public string Method { get; init; }
            
            public string Path { get; init; }
            
            public string Authorization { get; init; }

            public override string ToString()
            {
                return $"id: {Id} method: {Method} path: {Path}";
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
