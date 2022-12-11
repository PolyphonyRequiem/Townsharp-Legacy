using Townsharp.Consoles;
using Townsharp.Groups;
using Townsharp.Servers;
using Townsharp.Subscriptions;

namespace Townsharp
{
    // NOTE: Session is a service implementation in DDD jargon, not an aggregate root.
    public class Session
    {
        private readonly TownsharpConfig config;
        private readonly GroupManager groupManager;
        private readonly ServerManager serverManager;
        private readonly SubscriptionManager subscriptionManager;
        private readonly ConsoleSessionManager consoleSessionManager;

        public Session(
            TownsharpConfig config, 
            GroupManager groupsManager, 
            ServerManager serversManager, 
            SubscriptionManager subscriptionsManager, 
            ConsoleSessionManager consoleSessionsManager)
        {
            this.config = config;
            this.groupManager = groupsManager;
            this.serverManager = serversManager;
            this.subscriptionManager = subscriptionsManager;
            this.consoleSessionManager = consoleSessionsManager;
        }

        public Task<Server> GetServer(ServerId serverId)
        {
            return this.serverManager.GetServer(serverId);
        }

        public async Task<Dictionary<ServerId, Server>> GetJoinedServersMap()
        {
            var joinedServerIds = await this.serverManager.GetJoinedServerIds();

            // NOTE : DESIGN ERROR
            // TODO : ARCH
            // This may be the WRONG asynchronous pattern and will QUICKLY end up throttled, fix this before moving into any scalability tests.
            var joinedServersMap = joinedServerIds
                .AsEnumerable()
                .AsParallel()
                .ToDictionary(id => id, id => this.serverManager.GetServer(id).Result);

            return joinedServersMap;
        }

        // get notified on invitations

        // subscribe to events at appropriate scopes

        // So let's distinguish servers from managed servers, consoles from managed consoles, and groups from managed groups.
        // You can query/access servers/groups/consoles, and even do tasks on then, but in practice it's not much more than a dumb state object that only updates in response to events.
        // Any state that could be volatile should be part of a "snapshot" of some sort to make it clear that the state is a snapshot and not a fixed thing.
        // Managed servers etc should have state exposed in a more "continuous" way, and we should even expose "last updated" semantics at whatever scope.
    }
}