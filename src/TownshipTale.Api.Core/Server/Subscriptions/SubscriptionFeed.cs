namespace TownshipTale.Api.Core.Server.Subscriptions
{
    public sealed class SubscriptionFeed
    { 
        // TODO: sourcegenerators? Don't use it like this.  Bad B

        public static SubscriptionFeed TraceLog { get; } = new SubscriptionFeed("TraceLog");

        public static SubscriptionFeed DebugLog { get; } = new SubscriptionFeed("DebugLog");

        public static SubscriptionFeed InfoLog { get; } = new SubscriptionFeed("InfoLog");

        public static SubscriptionFeed WarnLog { get; } = new SubscriptionFeed("WarnLog");

        public static SubscriptionFeed ErrorLog { get; } = new SubscriptionFeed("ErrorLog");

        public static SubscriptionFeed FatalLog { get; } = new SubscriptionFeed("FatalLog");

        public static SubscriptionFeed PlayerStateChanged { get; } = new SubscriptionFeed("PlayerStateChanged");

        public static SubscriptionFeed PlayerJoined { get; } = new SubscriptionFeed("PlayerJoined");

        public static SubscriptionFeed PlayerLeft { get; } = new SubscriptionFeed("PlayerLeft");

        public static SubscriptionFeed PlayerKilled { get; } = new SubscriptionFeed("PlayerKilled");

        public static SubscriptionFeed PopulationModified { get; } = new SubscriptionFeed("PopulationModified");

        public static SubscriptionFeed TradeDeckUsed { get; } = new SubscriptionFeed("TradeDeckUsed");

        public static SubscriptionFeed ObjectKilled { get; } = new SubscriptionFeed("ObjectKilled");

        public static SubscriptionFeed TrialStarted { get; } = new SubscriptionFeed("TrialStarted");

        public static SubscriptionFeed TrialFinished { get; } = new SubscriptionFeed("TrialFinished");

        public static SubscriptionFeed ProfilingData { get; } = new SubscriptionFeed("ProfilingData");

        public SubscriptionFeed(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
    }
}