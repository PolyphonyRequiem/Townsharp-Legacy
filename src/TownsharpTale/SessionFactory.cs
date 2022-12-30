using Townsharp.Consoles;
using Townsharp.Groups;
using Townsharp.Servers;
using Townsharp.Subscriptions;

namespace Townsharp
{
    public class SessionFactory
    {
        private readonly Func<GroupManager> groupManagerFactory;
        private readonly Func<ServerManager> serverManagerFactory;
        private readonly Func<SubscriptionService> subscriptionServiceFactory;
        private readonly Func<ConsoleSessionService> consoleSessionServiceFactory;

        protected SessionFactory(
            Func<GroupManager> groupManagerFactory,
            Func<ServerManager> serverManagerFactory,
            Func<SubscriptionService> subscriptionServiceFactory,
            Func<ConsoleSessionService> consoleSessionServiceFactory) 
        {
            this.groupManagerFactory = groupManagerFactory;
            this.serverManagerFactory = serverManagerFactory;
            this.subscriptionServiceFactory = subscriptionServiceFactory;
            this.consoleSessionServiceFactory = consoleSessionServiceFactory;
        }

        public Session CreateConnectedSession(TownsharpConfig config)
        {
            return Session.Connect(
                config,
                groupManagerFactory(),
                serverManagerFactory(),
                subscriptionServiceFactory(),
                consoleSessionServiceFactory());
        }
    }
}
