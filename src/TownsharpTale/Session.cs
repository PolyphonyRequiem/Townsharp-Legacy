using Townsharp.Consoles;
using Townsharp.Groups;
using Townsharp.Servers;
using Townsharp.Subscriptions;

namespace Townsharp
{
    public class Session
    {
        private readonly TownsharpConfig config;
        private readonly GroupsManager groupsManager;
        private readonly ServersManager serversManager;
        private readonly SubscriptionsManager subscriptionsManager;
        private readonly ConsoleSessionsManager consoleSessionsManager;

        public Session(
            TownsharpConfig config, 
            GroupsManager groupsManager, 
            ServersManager serversManager, 
            SubscriptionsManager subscriptionsManager, 
            ConsoleSessionsManager consoleSessionsManager)
        {
            this.config = config;
            this.groupsManager = groupsManager;
            this.serversManager = serversManager;
            this.subscriptionsManager = subscriptionsManager;
            this.consoleSessionsManager = consoleSessionsManager;
        }

        public Task<Server> GetServer(ServerId serverId)
        {
            return this.serversManager.GetServer(serverId);
        }

        public async Task<Dictionary<ServerId, Server>> GetJoinedServersMap()
        {
            var joinedServerIds = await this.serversManager.GetJoinedServerIds();

            // NOTE : DESIGN ERROR
            // TODO : ARCH
            // This may be the WRONG asynchronous pattern and will QUICKLY end up throttled, fix this before moving into any scalability tests.
            var joinedServersMap = joinedServerIds
                .AsEnumerable()
                .AsParallel()
                .ToDictionary(id => id, id => this.serversManager.GetServer(id).Result);

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