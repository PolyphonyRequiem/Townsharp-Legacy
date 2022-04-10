namespace TownshipTale.Api.Core
{
    public class Group
    {
        public IEnumerable<Player> Players { get; }

        public IEnumerable<Server> Servers { get; }

        public IEnumerable<SubscriptionFeed> Subscriptions { get; }
    }
}
