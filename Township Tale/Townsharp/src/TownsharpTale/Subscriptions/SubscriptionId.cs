namespace Townsharp.Servers
{
    public record SubscriptionId
    {
        private readonly string id;

        public SubscriptionId(string id)
        {
            this.id = id;
        }

        public static implicit operator SubscriptionId(string id)
            => new SubscriptionId(id);

        public static implicit operator string(SubscriptionId subscriptionId)
            => subscriptionId.id;
    }
}
