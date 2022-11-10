using Microsoft.Extensions.Logging;
using Townsharp.Infra.Alta.Api;
using Townsharp.Infra.Alta.Console;
using Townsharp.Infra.Alta.Subscriptions;
using Townsharp.Servers;

public class ConsoleManager
{
    private readonly ApiClient apiClient;
    private readonly SubscriptionClient subscriptionClient;
    private readonly ConsoleClientFactory consoleClientFactory;
    private readonly ILogger<ConsoleManager> logger;

    private readonly HashSet<ServerId> managedConsoles = new HashSet<ServerId>();

    public ConsoleManager(ApiClient apiClient, SubscriptionClient subscriptionClient, ConsoleClientFactory consoleClientFactory, ILogger<ConsoleManager> logger)
	{
        // NOTE: we should be using "higher abstractions" for these things.
        this.apiClient = apiClient;
        this.subscriptionClient = subscriptionClient;
        this.consoleClientFactory = consoleClientFactory;
        this.logger = logger;
    }

    public async Task<bool> AddManagedConsole(ServerId serverId)
    {
        // verify that we can add a managed console for this particular server.
        // if we haven't already added it...
        // we should probably make sure this is thread safe. queued intent?
        if (this.managedConsoles.Contains(serverId))
        {
            this.logger.LogWarning($"Managed console registration already exists for {serverId}.");
            return true;
        }
        else
        {
            // we need to have access to this server
            // NOTE: Here's an example of where we need the higher abstraction.  We should be able to just say "hey, this server, what's our access to it like?" This is what the Server abstraction should be for.
            var serverInfo = await this.apiClient.GetServerInfo(serverId);
            // we need to not already be managing it
            // we need to verify we have console access
        }


        // let's subscribe to the event feed (probably need cross component messaging to handle this in the long run.)
        // open the console session if the server is live
        // 
    }

    // NOTE: Consider full Fluent syntax when migrating to domain models.
    // IE:
    // var commandresult = session.WithConsole(serverId)
    //                            .RunCommand(somecommand)
    public async Task<ManagedConsoleResult<TResult>> WithConsole<TResult>(ServerId serverId, Func<ConsoleClient, TResult> consoleAction)
    { 
        // if we
    }
}