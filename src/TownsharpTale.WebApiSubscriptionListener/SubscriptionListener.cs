using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Townsharp;

public class SubscriptionListener : IHostedService
{
    private readonly Session session;
    private readonly ILogger<SubscriptionListener> logger;

    public SubscriptionListener(Session session, ILogger<SubscriptionListener> logger)
    {
        this.session = session;
        this.logger = logger;
        this.session.SessionReady += OnSessionReady;
    }

    private void OnSessionReady(SessionReadyEvent obj)
    {
        logger.LogInformation("Session Ready!");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return this.session.Start(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}