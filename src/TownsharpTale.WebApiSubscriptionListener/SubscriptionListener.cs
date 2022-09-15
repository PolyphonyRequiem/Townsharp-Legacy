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

    public SubscriptionListener(Session session, ILogger<SubscriptionListener> logger, SubscriptionClient subscriptionClient, ApiClient apiClient)
    {
        this.session = session;
        this.logger = logger;
        this.subscriptionClient = subscriptionClient;
        this.apiClient = apiClient;
        this.session.SessionReady += OnSessionReady;

        ActivitySource.AddActivityListener(new ActivityListener()
        {
            ShouldListenTo = (source) => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = activity => Console.WriteLine("Started: {0,-15} {1,-60}", activity.OperationName, activity.Id),
            ActivityStopped = activity => Console.WriteLine("Stopped: {0,-15} {1,-60} {2,-15}", activity.OperationName, activity.Id, activity.Duration)
        });
    }

    private void OnSessionReady(SessionReadyEvent obj)
    {
        logger.LogInformation("Session Ready!");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation($"Subscribing to GroupStatusChanged events");
        this.subscriptionClient.GroupStatusChanged.Subscribe(message => this.logger.LogInformation(message.ToString()));

        using (Activity startup = activitySource.StartActivity("Listener Startup", ActivityKind.Client)!)
        {
            var joinedGroups = await this.apiClient.GetJoinedGroups();

            using (Activity subscribe = activitySource.StartActivity("Subscribing", ActivityKind.Client)!)
            {
                await Parallel.ForEachAsync(joinedGroups, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async (g, token) =>
                {
                    try
                    {
                        await this.subscriptionClient.SendMessage(HttpMethod.Post, $"subscription/group-server-status/{g.Group.Id}");
                    }
                    catch (System.TimeoutException)
                    {
                        // ignore it, it happens.
                    }
                });

                this.logger.LogInformation($"SEND: {this.subscriptionClient.sendTotal} RECV: {this.subscriptionClient.receiveTotal} ERROR: {this.subscriptionClient.errorTotal}"); 
            }
        }
                
        await Task.Delay(-1);
        //return this.session.Start(cancellationToken);        
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}