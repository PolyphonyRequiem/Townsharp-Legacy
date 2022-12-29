using Townsharp.Consoles;
using Townsharp.Groups;
using Townsharp.Servers;
using Townsharp.Subscriptions;

namespace Townsharp
{
    public class SessionFactory
    {
        private readonly Func<IGroupStatusProvider> groupStatusProviderFactory;
        private readonly Func<IServerStatusProvider> serverStatusProviderFactory;
        private readonly Func<SubscriptionService> subscriptionServiceFactory;
        private readonly Func<ConsoleSessionService> consoleSessionServiceFactory;

        protected SessionFactory(
            Func<IGroupStatusProvider> groupStatusProviderFactory,
            Func<IServerStatusProvider> serverStatusProviderFactory,
            Func<SubscriptionService> subscriptionServiceFactory,
            Func<ConsoleSessionService> consoleSessionServiceFactory) 
        {
            this.groupStatusProviderFactory = groupStatusProviderFactory;
            this.serverStatusProviderFactory = serverStatusProviderFactory;
            this.subscriptionServiceFactory = subscriptionServiceFactory;
            this.consoleSessionServiceFactory = consoleSessionServiceFactory;
        }

        public Session CreateConnectedSession(TownsharpConfig config)
        {
            return Session.Connect(
                config,
                groupStatusProviderFactory(),
                serverStatusProviderFactory(),
                subscriptionServiceFactory(),
                consoleSessionServiceFactory());
        }
    }
}
