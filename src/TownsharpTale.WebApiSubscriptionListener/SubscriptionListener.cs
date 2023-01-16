using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Townsharp;
using Townsharp.Api;
using Townsharp.Subscriptions;

public class SubscriptionListener : IHostedService
{
    private readonly ILogger<SubscriptionListener> logger;
    private readonly SubscriptionClient subscriptionClient;
    private readonly ApiClient apiClient;
    private readonly ActivitySource activitySource = new ActivitySource(nameof(SubscriptionListener));
    
    private int total = 0;

    public SubscriptionListener(ILogger<SubscriptionListener> logger, SubscriptionClient subscriptionClient, ApiClient apiClient)
    {
        this.logger = logger;
        this.subscriptionClient = subscriptionClient;
        this.apiClient = apiClient;

        //ActivitySource.AddActivityListener(new ActivityListener()
        //{
        //    ShouldListenTo = (source) => true,
        //    Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllDataAndRecorded,
        //    ActivityStarted = activity => Console.WriteLine("Started: {0,-15} {1,-60}", activity.OperationName, activity.Id),
        //    ActivityStopped = activity => Console.WriteLine("Stopped: {0,-15} {1,-60} {2,-15}", activity.OperationName, activity.Id, activity.Duration)
        //});
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation($"Subscribing to GroupStatusChanged events");

        this.subscriptionClient.SubscriptionEventReceived.Subscribe(@event =>
        {
            this.logger.LogInformation(@event.Content);
            this.total++;
        });

        await this.subscriptionClient.Run(OnConnected, OnFaulted);

        async Task OnConnected()
        {
            string subscriptionEventKey = "group-server-status";
            List<Task<SubscriptionRequestResult>> subscriptionTasks = new List<Task<SubscriptionRequestResult>>();

            //await Parallel.ForEachAsync(joinedGroups, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async (g, token) =>
            //{
            //})

            await foreach (ApiJoinedGroup joinedGroup in this.apiClient.GetJoinedGroups())
            {
                var groupId = joinedGroup.Group.Id.ToString();
                this.logger.LogInformation($"Subscribing to '{subscriptionEventKey}' on '{groupId}'");
                subscriptionTasks.Add(this.subscriptionClient.Subscribe(subscriptionEventKey, groupId));
            }

            Task.WaitAll(subscriptionTasks.ToArray());
        }

        void OnFaulted()
        {
            Debugger.Break();
        }

        //using (activitySource.StartActivity("Listener Startup", ActivityKind.Client)!)
        //{
        //    using (activitySource.StartActivity("Subscribing", ActivityKind.Client)!)
        //    {
        //        await foreach (var joinedGroup in )
        //        {
                    
                  
        //        };
        //    }
        //}

        _ = Task.Run(() => PeriodicUpdate());
        await Task.Delay(-1);
    }

    public async Task PeriodicUpdate()
    {
        await Task.Delay(TimeSpan.FromMinutes(.5));
        this.logger.LogInformation($"TOTAL GroupStatusChanged events: {total}");
        _ = Task.Run(() => PeriodicUpdate());
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}