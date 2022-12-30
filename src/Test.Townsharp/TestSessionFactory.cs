using Townsharp.Consoles;
using Townsharp.Groups;
using Townsharp.Servers;
using Townsharp.Subscriptions;

namespace Test.Townsharp
{
    internal class TestSessionFactory : SessionFactory
    {
        public TestSessionFactory(
            Func<GroupManager> groupManagerFactory,
            Func<ServerManager> serverManagerFactory,
            Func<SubscriptionService> subscriptionServiceFactory,
            Func<ConsoleSessionService> consoleSessionServiceFactory)
            : base(groupManagerFactory, serverManagerFactory, subscriptionServiceFactory, consoleSessionServiceFactory)
        {

        }
    }
}