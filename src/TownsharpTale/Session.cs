using Townsharp.Consoles;
using Townsharp.Groups;
using Townsharp.Servers;
using Townsharp.Subscriptions;

namespace Townsharp
{

    // only place we can "make" groups and server objects.  Also manages console sessions and subscription events.
    // public class Session(GroupManager, ServerManager, ConsoleSessionService, SubscriptionService)

    // made by the GroupManager, is given a IGroupStatusProvider at creation.
    // public class Group(IGroupStatusProvider, Session)

    // made by the ServerManager, is given a IServerStatusProvider at creation.
    // public class Server(IServerStatusProvider, Session) 

    // made by (and accessed through) the ConsoleSessionService.  Dies when disconnected for any reason, and the ConsoleSessionService may yield new ConsoleSessions when the console reconnects
    // public class ConsoleSession(Server)

    // gets you access to groups and servers, and provides management over consoles and subscriptions.
    public class Session
    {
        private readonly TownsharpConfig config;
        private readonly GroupManager groupManager;
        private readonly ServerManager serverManager;
        private readonly SubscriptionService subscriptionService;
        private readonly ConsoleSessionService consoleSessionService;

        protected internal Session(
            TownsharpConfig config,
            GroupManager groupManager,
            ServerManager serverManager,
            SubscriptionService subscriptionService,
            ConsoleSessionService consoleSessionService)
        {
            this.config = config;
            this.groupManager = groupManager;
            this.serverManager = serverManager;
            this.subscriptionService = subscriptionService;
            this.consoleSessionService = consoleSessionService;
        }

        internal static Session Connect(
            TownsharpConfig config,
            GroupManager groupStatusProvider,
            ServerManager serverStatusProvider,
            SubscriptionService subscriptionService,
            ConsoleSessionService consoleSessionService)
            // Add room here for event handlers.
        {
            var session = new Session(
                config,
                groupStatusProvider,
                serverStatusProvider,
                subscriptionService,
                consoleSessionService);

            session.Initialize();

            return session;
        }

        public async Task<Server[]> GetJoinedServersAsync()
        {
            return await this.serverManager.GetJoinedServersAsync();
        }

        public async Task<Server> GetServerAsync(ServerId serverId)
        {
            return await this.serverManager.GetServerAsync(serverId);
        }

        private void Initialize()
        {
            if (this.config.AutoManageJoinedGroups == true) 
            {
                
            }
        }

        // get notified on invitations

        // subscribe to events at appropriate scopes

        // So let's distinguish servers from managed servers, consoles from managed consoles, and groups from managed groups.
        // You can query/access servers/groups/consoles, and even do tasks on then, but in practice it's not much more than a dumb state object that only updates in response to events.
        // Any state that could be volatile should be part of a "snapshot" of some sort to make it clear that the state is a snapshot and not a fixed thing.
        // Managed servers etc should have state exposed in a more "continuous" way, and we should even expose "last updated" semantics at whatever scope.
    }
}