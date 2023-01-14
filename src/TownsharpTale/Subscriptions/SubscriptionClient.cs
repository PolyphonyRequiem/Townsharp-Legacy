using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using static Townsharp.Api.Json.JsonUtils;

namespace Townsharp.Subscriptions
{
    /// <summary>
    /// Document the lifecycle and usage here, especially if you plan to share this publicly.
    /// Start reorganizing this and other bits along seperation of concern seams.
    /// </summary>
    public class SubscriptionClient
    {
        private static readonly Uri SubscriptionWebsocketUri = new Uri("wss://5wx2mgoj95.execute-api.ap-southeast-2.amazonaws.com/dev");

        private static readonly TimeSpan MigrationFrequency = TimeSpan.FromMinutes(110);

        private readonly Func<string> authTokenFactory;
        private readonly ILogger<SubscriptionClient> logger;

        private readonly IList<IDisposable> subscriptionHandles = new List<IDisposable>();

        private SubscriptionWebsocketClient? subscriptionWebsocketClient;
        private bool connected = true;
        private Task migrating = Task.CompletedTask;
        private Subject<long> startMigration = new Subject<long>();
        private IDisposable migrationTimerSubscription = Disposable.Empty;
        private Subject<int> onFaulted = new Subject<int>();

        private readonly Subject<SubscriptionEvent> subscriptionEventSubject = new Subject<SubscriptionEvent>();

        public IObservable<SubscriptionEvent> SubscriptionEventReceived => subscriptionEventSubject;

        public SubscriptionClient(
            Func<string> authTokenFactory,
            ILogger<SubscriptionClient> logger)
        {
            this.authTokenFactory = authTokenFactory;
            this.logger = logger;
        }

        public void Connect(Action connected, Action faulted)
        {
            if (this.connected == true)
            {
                throw new InvalidOperationException("Unable to connect, session is already connected.");
            }

            this.subscriptionWebsocketClient = CreateSubscriptionWebsocketClient(this.authTokenFactory);
            this.SubscribeAllForWebsocket(this.subscriptionWebsocketClient);
            this.onFaulted.Subscribe(_ =>
            {
                // what?  No.  This... Isn't... You can't just... Unless you're expecting them to call run again?  What's the lifecycle.  
                // We need to test this and you don't get to move on until you write tests mr. man!
                this.onFaulted = new Subject<int>();
                faulted();
            });
            this.RegisterForMigration();
            this.connected = true;
            connected();
        }

        public async Task<SubscriptionRequestResult> Subscribe(string eventname, string key, CancellationToken cancellationToken = default)
        {
            this.EnsureConnected();
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
            this.EnsureConnected();
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
            this.EnsureConnected();
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
            var newClient = CreateSubscriptionWebsocketClient(this.authTokenFactory);

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
                this.connected = false;
                this.onFaulted.OnNext(0);
                throw new SubscriptionClientMigrationException(); // fails the Task, and by extension any awaiting callers.
            }
        }

        private void SubscribeAllForWebsocket(SubscriptionWebsocketClient client)
        {

            var newSubscription = client.SubscriptionEventReceived.Subscribe(OnSubscriptionEventReceived);

            foreach (var retiredHandle in this.subscriptionHandles)
            {
                retiredHandle.Dispose();
            }

            this.subscriptionHandles.Add(newSubscription);
        }

        // Actually we should probably split the stream and republish! How exciting is that :)
        private void OnSubscriptionEventReceived(SubscriptionEvent subscriptionEventReceived) 
            => this.subscriptionEventSubject.OnNext(subscriptionEventReceived);
        
        private void EnsureConnected([CallerMemberName] string caller = "")
        {
            if (!this.connected)
            {
                throw new InvalidOperationException($"Unable to call {caller}, not yet connected.  Please call Connect() first.");
            }
        }
        private static SubscriptionWebsocketClient CreateSubscriptionWebsocketClient(Func<string> authTokenFactory)
        {
            return new SubscriptionWebsocketClient(SubscriptionWebsocketUri, () => authTokenFactory.Invoke());
        }

        protected record MigrationTokenContent
        {
            public string token { get; init; } = string.Empty;
        }
    }
}