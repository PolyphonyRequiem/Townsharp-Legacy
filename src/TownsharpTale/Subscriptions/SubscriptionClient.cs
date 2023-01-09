//using Polly;
//using System.Net.WebSockets;
//using System.Reactive.Linq;
//using System.Reactive.Subjects;
//using System.Reactive.Threading.Tasks;
//using static Townsharp.Api.Json.JsonUtils;
//using Websocket.Client;
//using Websocket.Client.Models;

//namespace Townsharp.Subscriptions
//{
//    internal class SubscriptionClient
//    {
//        private static readonly AsyncPolicy<SendMessageResult> messageRetryPolicy = Policy<SendMessageResult>.Handle<TimeoutException>().RetryAsync(2);
//        private static readonly Uri SubscriptionWebsocketUri = new Uri("wss://5wx2mgoj95.execute-api.ap-southeast-2.amazonaws.com/dev");

//        private static readonly TimeSpan ResponseTimeout = TimeSpan.FromSeconds(30);

//        private readonly Func<string> authTokenFactory;
//        private readonly Action<SubscriptionEvent> subscriptionEventHandler;
//        private readonly Action<SubcriptionClientConnectedEvent> connectedHandler;
//        private readonly Action<SubscriptionClientFaultedEvent> faultedHandler;
//        private readonly Action<SubscriptionClientMigrationStartedEvent> migrationHandler;
//        private readonly Action<SubscriptionClientMigrationCompletedEvent> migratedHandler;

//        private SubscriptionWebsocketClient? subscriptionWebsocketClient;

//        private ISubject<SubscriptionOperationRequest> subscriptionOperationRequests = new Subject<SubscriptionOperationRequest>();
//        private ISubject<SubscriptionRequestResult> subscriptionRequestResults = new Subject<SubscriptionRequestResult>();

        

//        private SubscriptionClient(
//            Func<string> authTokenFactory,
//            Action<SubscriptionEvent> subscriptionEventHandler,
//            Action<SubcriptionClientConnectedEvent> connectedHandler,
//            Action<SubscriptionClientFaultedEvent> faultedHandler,
//            Action<SubscriptionClientMigrationStartedEvent> migrationHandler,
//            Action<SubscriptionClientMigrationCompletedEvent> migratedHandler)
//        {
//            this.authTokenFactory = authTokenFactory;
//            this.subscriptionEventHandler = subscriptionEventHandler;
//            this.connectedHandler = connectedHandler;
//            this.faultedHandler = faultedHandler;
//            this.migrationHandler = migrationHandler;
//            this.migratedHandler = migratedHandler;
//        }

//        public static async Task<SubscriptionClient> StartAsync(// subscription client factory?
//            Func<string> authTokenFactory,
//            Action<SubscriptionEvent> subscriptionEventHandler,
//            Action<SubcriptionClientConnectedEvent> connectedHandler,
//            Action<SubscriptionClientFaultedEvent> faultedHandler,
//            Action<SubscriptionClientMigrationStartedEvent> migrationHandler,
//            Action<SubscriptionClientMigrationCompletedEvent> migratedHandler)
//        {
//            var client = new SubscriptionClient(
//                authTokenFactory,
//                subscriptionEventHandler,
//                connectedHandler,
//                faultedHandler,
//                migrationHandler,
//                migratedHandler);

//            client.subscriptionWebsocketClient = await client.CreateSubscriptionWebsocketClientAsync();

//            return client;
//        }

//        public Task<SubscriptionRequestResult> Subscribe(SubscriptionRequest request, CancellationToken cancellationToken)
//        {
//            var id = this.messageIdFactory.GetNext();
//            var subscriptionRequest = new SubscriptionOperationRequest(id, SubscriptionOperation.Subscribe, request);
//            return this.SendSubscriptionOperationRequest(subscriptionRequest, cancellationToken);
//        }

//        public Task<SubscriptionRequestResult> Unsubscribe(SubscriptionRequest request, CancellationToken cancellationToken)
//        {
//            var id = this.messageIdFactory.GetNext();
//            var subscriptionRequest = new SubscriptionOperationRequest(id, SubscriptionOperation.Unsubscribe, request);
//            return this.SendSubscriptionOperationRequest(subscriptionRequest, cancellationToken);
//        }

//        private Task<SubscriptionRequestResult> SendSubscriptionOperationRequest(SubscriptionOperationRequest request, CancellationToken cancellationToken)
//        {
//            var id = this.messageIdFactory.GetNext();
//            var subscriptionRequestResultTask =
//                this.subscriptionRequestResults
//                    .FirstAsync(result => result.MessageId == id)
//                    .ToTask(cancellationToken);

//            this.subscriptionOperationRequests.OnNext(request);
//            return subscriptionRequestResultTask;
//        }

//        private async Task SendSubscriptionRequest(WebsocketClient client, string eventname, string key) =>
//            await messageRetryPolicy.ExecuteAsync(async () =>
//                await this.SendMessageAsync(new SubscriptionClientMessage(HttpMethod.Post, $"subscription/{eventname}/{key}")));

//        private async Task SendUnscriptionRequest(WebsocketClient client, string eventname, string key) =>
//            await messageRetryPolicy.ExecuteAsync(async () =>
//                await this.SendMessageAsync(new SubscriptionClientMessage(HttpMethod.Delete, $"subscription/{eventname}/{key}")));


//        private Task<SendMessageResult> SendMessageAsync(SubscriptionClientMessage message)
//        {
//            //            if (state == State.Disconnected)
//            //            {
//            //                throw new InvalidOperationException("Unable to send messages on a disconnected client.");
//            //            }

//            //            // Delay outgoing messages (unless path is migrate)
//            //            if (path != "migrate")
//            //            {
//            //                await migrationTaskSource.Task;
//            //            }

//            //            var id = messageId++;
//            //            SubscriptionRequestMessage message = new SubscriptionRequestMessage(id, method, path, authToken!, content);
//            //            logger.LogDebug(message.ToString());

//            //            // Prepare to receive single response.
//            //            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();

//            //            client
//            //                .MessageReceived
//            //                .Where(response => IsResponseMessageForId(response, id))
//            //                .Take(1)
//            //                .Timeout(ResponseTimeout)
//            //                .Subscribe(response =>
//            //                {
//            //                    logger.LogDebug($"Got expected response for id: {id}.{Environment.NewLine}{response.Text}");
//            //                    tcs.SetResult(response.Text);
//            //                },
//            //                exception =>
//            //                {
//            //                    logger.LogWarning(exception, $"An error occurred while awaiting response for id: {id}");
//            //                    tcs.SetException(exception);
//            //                });

//            //            var messageText = Serialize(message);
//            //            client.Send(messageText);

//            //            return await tcs.Task;
//        }


//        private async Task<SubscriptionWebsocketClient> CreateSubscriptionWebsocketClientAsync()
//        {
//            var client = new SubscriptionWebsocketClient(SubscriptionWebsocketUri, () =>
//                {
//                    var client = new ClientWebSocket();
//                    client.Options.KeepAliveInterval = TimeSpan.FromMinutes(15);
//                    client.Options.SetRequestHeader("Authorization", $"Bearer {this.authTokenFactory.Invoke()}");
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
//            return client;
//        }


//        private void MessageReceived(ResponseMessage responseMessage)
//        {

//        }

//        private void DisconnectionHappened(DisconnectionInfo disconnectionInfo)
//        {

//        }

//        private void ReconnectionHappened(ReconnectionInfo reconnectionInfo)
//        {

//        }

//        private async Task<string> SendMessageForClient(WebsocketClient client, SubscriptionClientMessage message)
//        {            
//            // Prepare to receive single response.
//            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
//            SubscriptionRequestMessage request = new SubscriptionRequestMessage(id, message.Method, message.Path, this.AuthToken, message.Content);

//            client
//                .MessageReceived
//                .Where(response => IsResponseMessageForId(response, id))
//                .Take(1)
//                .Timeout(ResponseTimeout)
//                .Subscribe(response =>
//                {
//                    tcs.SetResult(response.Text);
//                },
//                exception =>
//                {
//                    tcs.SetException(exception);
//                });

//            var requestText = JsonUtils.Serialize(request);
//            client.Send(requestText);

//            return await tcs.Task;
//        }

//        protected record MigrationTokenContent
//        {
//            public string Token { get; init; } = string.Empty;
//        }

//        protected record ErrorContent
//        {
//            public string Message { get; init; } = string.Empty;

//            public string ErrorCode { get; init; } = string.Empty;
//        }

//        private enum State
//        {
//            Uninitialized,
//            Connecting,
//            Connected,
//            Migrating,
//            Faulted,
//            Disconnected
//        }

//        private enum SubscriptionOperation
//        {
//            Subscribe,
//            Unsubscribe
//        } 

//        private record SubscriptionOperationRequest(long MessageId, SubscriptionOperation Operation, SubscriptionRequest Request);
//    }
//}