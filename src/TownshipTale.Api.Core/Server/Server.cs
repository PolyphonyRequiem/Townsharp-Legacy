using TownshipTale.Api.Core.Server.Console;
using TownshipTale.Api.Core.Server.Subscriptions;

namespace TownshipTale.Api.Core.Server
{
    public class Server
    {
        internal Server(SubscriptionManager subscriptionManager, ConsoleSession console)
        {
            this.Subscriptions = subscriptionManager;
            this.Console = console;
        }

        private SubscriptionManager Subscriptions { get; }

        private ConsoleSession Console { get; }


        //public SubscriptionContext Subscribe(SubscriptionFeed feed)
        //{
        //    //this.SubscriptionManager.
        //}
    }
}