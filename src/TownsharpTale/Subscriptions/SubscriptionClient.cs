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
        private IDisposable migrationTimerSubscription = Disposable.Empty;
        private bool isConnected = false;
        private Task migrating = Task.CompletedTask;
        private Subject<long> startMigration = new Subject<long>();
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

        //
        // CONSIDER ELEVATING MIGRATION LOGIC TO THE APPLICATION LEVEL
        // MIGRATION TOKEN CAN BE FETCHED HERE OR ISSUED ON FABRICATION.
        //

        public async Task Run(Func<Task> onConnected, Action onFaulted)
        {
            if (this.isConnected == true)
            {
                throw new InvalidOperationException("Unable to connect, session is already connected.");
            }

            this.subscriptionWebsocketClient = CreateSubscriptionWebsocketClient(this.authTokenFactory);
            this.SubscribeAllForWebsocket(this.subscriptionWebsocketClient);
            this.onFaulted
                .Take(1)
                .Subscribe(_ => {
                    onFaulted();
                    this.migrationTimerSubscription.Dispose();
                });
            this.RegisterForMigration();
            this.isConnected = true;
            await onConnected.Invoke();
        }

        public Task<SubscriptionRequestResult> Subscribe(string eventname, string key, CancellationToken cancellationToken = default)
            => this.HandleSubscription(HttpMethod.Post, eventname, key, cancellationToken);

        public Task<SubscriptionRequestResult> Unsubscribe(string eventname, string key, CancellationToken cancellationToken) 
            => this.HandleSubscription(HttpMethod.Delete, eventname, key, cancellationToken);

        private async Task<SubscriptionRequestResult> HandleSubscription(HttpMethod method, string eventname, string key, CancellationToken cancellationToken)
        {
            this.EnsureConnected();
            await this.migrating;
            var response = await this.subscriptionWebsocketClient!.SendRequestAsync(method, $"subscription/{eventname}/{key}", cancellationToken: cancellationToken);

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
            if (this.isConnected)
            {
                this.migrating = Task.Run(MigrateWebsocket);
            }
        }

        private async Task MigrateWebsocket()
        {
            var oldClient = this.subscriptionWebsocketClient!;
            var newClient = CreateSubscriptionWebsocketClient(this.authTokenFactory);

            var result = await oldClient.SendRequestAsync(HttpMethod.Get, "migrate");

            bool success = await result.Match(
                async message =>
                {
                    var migrationToken = Deserialize<MigrationTokenContent>(message.Content).Value;
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
                this.HandleMigrationFaulted();
            }
        }

        private void HandleMigrationFaulted()
        {
            this.isConnected = false;
            this.onFaulted.OnNext(0);
            throw new SubscriptionClientMigrationException(); // fails the Task, and by extension any awaiting callers.
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

        private void OnSubscriptionEventReceived(SubscriptionEvent subscriptionEventReceived) 
            => this.subscriptionEventSubject.OnNext(subscriptionEventReceived);
        
        private void EnsureConnected([CallerMemberName] string caller = "")
        {
            if (!this.isConnected)
            {
                throw new InvalidOperationException($"Unable to call {caller}, not yet connected.  Please call Connect() first.");
            }
        }

        private static SubscriptionWebsocketClient CreateSubscriptionWebsocketClient(Func<string> authTokenFactory)
        {
            return new SubscriptionWebsocketClient(SubscriptionWebsocketUri, () => authTokenFactory.Invoke());
        }

        private record MigrationTokenContent
        {
            public string token { get; init; } = string.Empty;
        }
    }
}