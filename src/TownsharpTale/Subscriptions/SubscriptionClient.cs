using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using Websocket.Client;
using static Townsharp.Api.Json.JsonUtils;

namespace Townsharp.Subscriptions
{
    public class SubscriptionClient
    {
        private static readonly Uri SubscriptionWebsocketUri = new Uri("wss://5wx2mgoj95.execute-api.ap-southeast-2.amazonaws.com/dev");

        private static readonly TimeSpan MigrationFrequency = TimeSpan.FromMinutes(.5);

        private readonly Func<string> authTokenFactory;
        private readonly ILogger<SubscriptionClient> logger;
        private SubscriptionWebsocketClient? subscriptionWebsocketClient;
        private bool initialized = true;
        private Task migrating = Task.CompletedTask;
        private Subject<long> startMigration = new Subject<long>();
        private IDisposable migrationTimerSubscription = Disposable.Empty;
        private Subject<int> onFaulted = new Subject<int>();

        private readonly Subject<SubscriptionEvent> subscriptionEventSubject = new Subject<SubscriptionEvent>();

        // this is not the right pattern, each call with request a new Observable sequence.
        public IObservable<SubscriptionEvent> SubscriptionEventReceived => subscriptionEventSubject.AsObservable();

        public SubscriptionClient(
            Func<string> authTokenFactory,
            ILogger<SubscriptionClient> logger)
        {
            this.authTokenFactory = authTokenFactory;
            this.logger = logger;
        }

        public async Task Run(Action connected, Action faulted)
        {
            this.subscriptionWebsocketClient = await CreateConnectedSubscriptionWebsocketClientAsync(this.authTokenFactory);
            this.SubscribeAllForWebsocket(this.subscriptionWebsocketClient);
            this.onFaulted.Subscribe(_ =>
            {
                // what?  No.  This... Isn't... You can't just... Unless you're expecting them to call run again?  What's the lifecycle.  
                // We need to test this and you don't get to move on until you write tests mr. man!
                this.onFaulted = new Subject<int>();
                faulted();
            });
            this.RegisterForMigration();
            this.initialized = true;
            connected();
        }

        public async Task<SubscriptionRequestResult> Subscribe(string eventname, string key, CancellationToken cancellationToken = default)
        {
            this.EnsureInitialized();
            // These really should be in some way "queued" or managed.
            // Figure out lifecycle and how to model it before figuring out operational details and dataflow.
            await this.migrating;
            var response = await this.subscriptionWebsocketClient!.SendRequestAsync(HttpMethod.Post, $"subscription/{eventname}/{key}", cancellationToken: cancellationToken);

            return response.Match(
                response => SubscriptionRequestResult.Success(),
                error => SubscriptionRequestResult.Fail(error.Message));
        }

        public async Task<SubscriptionRequestResult> Unsubscribe(string eventname, string key, CancellationToken cancellationToken)
        {
            this.EnsureInitialized();
            // These really should be in some way "queued" or managed
            // Figure out lifecycle and how to model it before figuring out operational details and dataflow.
            await this.migrating;
            var response = await this.subscriptionWebsocketClient!.SendRequestAsync(HttpMethod.Delete, $"subscription/{eventname}/{key}", cancellationToken: cancellationToken);

            return response.Match(
                response => SubscriptionRequestResult.Success(),
                error => SubscriptionRequestResult.Fail(error.Message));
        }

        public void ForceMigration()
        {
            this.EnsureInitialized();
            this.startMigration.OnNext(0);
        }
                
        private void RegisterForMigration()
        {
            this.migrationTimerSubscription.Dispose();
            this.migrationTimerSubscription = Observable.Merge(startMigration, Observable.Timer(MigrationFrequency))
                .Subscribe(_ => this.StartWebsocketMigration());
        }

        private void StartWebsocketMigration()
        {
            this.migrating = Task.Run(MigrateWebsocket);
        }

        private async Task MigrateWebsocket()
        {
            // Create a new websocket client
            var oldClient = this.subscriptionWebsocketClient!;
            var newClient = await CreateConnectedSubscriptionWebsocketClientAsync(this.authTokenFactory);

            var result = await oldClient.SendRequestAsync(HttpMethod.Get, "migrate");

            bool success = await result.Match(
                async message =>
                {
                    var migrationToken = Deserialize<MigrationTokenContent>(message.Content).Value;

                    // okay, let's do this!  new Client is a go.
                    // need to clean up subscriptions? possible leak?
                    this.SubscribeAllForWebsocket(newClient);

                    var result = await newClient.SendRequestAsync(HttpMethod.Post, "migrate", Serialize(migrationToken));
                    return result.IsSuccess;
                },
                error =>
                {
                    return Task.FromResult(false);
                }
            );

            if (success)
            {
                this.subscriptionWebsocketClient = newClient;
                this.RegisterForMigration();
                oldClient.Dispose();
            }
            else
            {
                this.onFaulted.OnNext(0);
                throw new SubscriptionClientMigrationException(); // fails the Task, and by extension any awaiting callers.
            }            
        }

        private void SubscribeAllForWebsocket(SubscriptionWebsocketClient client)
        {
            client.SubscriptionEventReceived.Subscribe(OnSubscriptionEventReceived);
            client.MessageReceived.Subscribe(OnMessageReceived);
            client.DisconnectionHappened.Subscribe(OnDisconnectionHappened);
        }

        private void OnSubscriptionEventReceived(SubscriptionEvent subscriptionEventReceived)
        {
            // Actually we should probably split the stream and republish! How exciting is that :)
            this.subscriptionEventSubject.OnNext(subscriptionEventReceived);
        }

        private void OnMessageReceived(ResponseMessage responseMessage)
        {

        }

        private void OnDisconnectionHappened(DisconnectionInfo disconnectionInfo)
        {

        }

        private void EnsureInitialized([CallerMemberName] string caller = "")
        {
            if (!initialized)
            {
                throw new InvalidOperationException($"Unable to call {caller} from  ");
            }
        }

        private static async Task<SubscriptionWebsocketClient> CreateConnectedSubscriptionWebsocketClientAsync(Func<string> authTokenFactory)
        {
            var client = new SubscriptionWebsocketClient(SubscriptionWebsocketUri, () => authTokenFactory.Invoke());
            await client.Start();
            return client;
        }

        protected record MigrationTokenContent
        {
            public string token { get; init; } = string.Empty;
        }
    }
}