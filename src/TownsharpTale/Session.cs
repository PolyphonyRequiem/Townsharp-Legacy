using Townsharp.Consoles;
using Townsharp.Groups;
using Townsharp.Servers;
using Townsharp.Subscriptions;

namespace Townsharp
{
    public class Session
    {
        public TownsharpConfig Config { get; }

        private readonly GroupManager groupManager;
        private readonly ServerManager serverManager;
        private readonly SubscriptionService subscriptionService;
        private readonly ConsoleSessionService consoleSessionService;

        private bool isStarted = false;

        public GroupManager GroupManager => IfStarted(() => this.groupManager);

        public ServerManager ServerManager => IfStarted(() => this.serverManager);

        public SubscriptionService SubscriptionService => IfStarted(() => this.subscriptionService);

        public ConsoleSessionService ConsoleSessionService => IfStarted(() => this.consoleSessionService);

        private T IfStarted<T>(Func<T> function)
        {
            if (this.isStarted)
            {
                return (function());
            }
            else
            {
                throw new SessionNotReadyException("This action is not available on an unstarted session.  Make sure Session was created by a SessionFactory");
            }
        }

        protected internal Session(
            TownsharpConfig config,
            GroupManager groupManager,
            ServerManager serverManager,
            SubscriptionService subscriptionService,
            ConsoleSessionService consoleSessionService)
        {
            this.Config = config;
            this.groupManager = groupManager;
            this.serverManager = serverManager;
            this.subscriptionService = subscriptionService;
            this.consoleSessionService = consoleSessionService;
        }
         
        internal void Start()
        {
            if (this.Config.AutoManageJoinedGroups == true)
            {
                // do stuff
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