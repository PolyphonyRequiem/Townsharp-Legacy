using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Townsharp.Api;
using Townsharp.Api.Identity;
using Townsharp.Subscriptions;

internal class SubscriptionClientTest : IHostedService
{
    private readonly ILogger<SubscriptionClientTest> logger;
    private readonly ApiClient apiClient;
    private readonly AccountsTokenClient accountsTokenClient;
    private readonly ILoggerFactory loggerFactory;

    public SubscriptionClientTest(
        ILogger<SubscriptionClientTest> logger,
        ApiClient apiClient,
        AccountsTokenClient accountsTokenClient,
        ILoggerFactory loggerFactory)
    {
        this.logger = logger;
        this.apiClient = apiClient;
        this.accountsTokenClient = accountsTokenClient;
        this.loggerFactory = loggerFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var client = new SubscriptionClient(() => this.accountsTokenClient.GetValidToken().Result.AccessToken!, loggerFactory.CreateLogger<SubscriptionClient>());
        await client.Run(Connected, Faulted);

        client.SubscriptionEventReceived.Subscribe(groupServerStatusChangedEvent => this.logger.LogInformation(groupServerStatusChangedEvent.Content));

        void Faulted()
        {
            this.logger.LogInformation("Faulted!");
        }

        void Connected()
        {
            Task.Run(async () =>
            {
                await foreach (var server in this.apiClient.GetJoinedServersAsync())
                {
                    await client.Subscribe("group-server-status", server.GroupId.ToString());
                }
            });
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}