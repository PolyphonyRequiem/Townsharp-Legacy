using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Townsharp;
using Townsharp.Infra.Alta.Api;
using Townsharp.Infra.Alta.Subscriptions;

public class SubscriptionListener : IHostedService
{
    private readonly Session session;
    private readonly ILogger<SubscriptionListener> logger;
    private readonly SubscriptionClient subscriptionClient;
    private readonly ApiClient apiClient;
    private readonly ActivitySource activitySource = new ActivitySource(nameof(SubscriptionListener));
    
    private int total = 0;

    public SubscriptionListener(Session session, ILogger<SubscriptionListener> logger, SubscriptionClient subscriptionClient, ApiClient apiClient)
    {
        this.session = session;
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
        
        //this.subscriptionClient.GroupStatusChanged.Subscribe(message => this.logger.LogInformation(message.ToString()));
        this.subscriptionClient.ServerStatusChanged.Subscribe(_ => this.total++);

        await this.subscriptionClient.Connect();

        using (activitySource.StartActivity("Listener Startup", ActivityKind.Client)!)
        {
            using (activitySource.StartActivity("Subscribing", ActivityKind.Client)!)
            {
                await foreach (var joinedGroup in this.apiClient.GetJoinedGroupDescriptions())
                {
                    await this.subscriptionClient.Subscribe("group-server-status", joinedGroup.Id.ToString());
                    //await Parallel.ForEachAsync(joinedGroups, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async (g, token) =>
                    //{
                        
                    //})
                };
            }
        }

        _ = Task.Run(() => PeriodicUpdate());
        await Task.Delay(-1);
        //return this.session.Start(cancellationToken);        
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