namespace TownshipTale.Api.Core
{
    public class Group
    {
        public int Id { get; }

        public string Name { get; }

        public IEnumerable<Member> Members { get; }

        public IEnumerable<Server> Servers { get; }

        public IEnumerable<SubscriptionFeed> Subscriptions { get; }
    }
}
