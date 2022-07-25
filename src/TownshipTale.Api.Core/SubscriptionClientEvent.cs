using TownshipTale.Api.Core.Utilities;

namespace TownshipTale.Api.Core
{
    // NOTE! Consider SourceGenerators for this?
    public record SubscriptionClientEvent
    {
        private SubscriptionClientEvent(string eventName)
        {
            EventName = eventName;
        }

        public string EventName { get; }

        public static SubscriptionClientEvent GroupMemberUpdate = new SubscriptionClientEvent("group-member-update");
        public static SubscriptionClientEvent GroupServerCreate = new SubscriptionClientEvent("group-server-create");
        public static SubscriptionClientEvent GroupServerDelete = new SubscriptionClientEvent("group-server-delete");
        public static SubscriptionClientEvent GroupServerStatus = new SubscriptionClientEvent("group-server-status");
        public static SubscriptionClientEvent GroupServerUpdate = new SubscriptionClientEvent("group-server-update");
        public static SubscriptionClientEvent GroupUpdate = new SubscriptionClientEvent("group-update");
        public static SubscriptionClientEvent MeGroupCreate = new SubscriptionClientEvent("me-group-create");
        public static SubscriptionClientEvent MeGroupDelete = new SubscriptionClientEvent("me-group-delete");
        public static SubscriptionClientEvent MeGroupInviteCreate = new SubscriptionClientEvent("me-group-invite-create");
        public static SubscriptionClientEvent MeGroupInviteDelete = new SubscriptionClientEvent("me-group-invite-delete");

        static SubscriptionClientEvent()
        {
            Values = DiscreteValuesRecordHelpers.GetStaticMappings<SubscriptionClientEvent>(_ => _.EventName);
        }

        private static Dictionary<string, SubscriptionClientEvent> Values;

        public static implicit operator SubscriptionClientEvent(string eventName)
        {
            return Values[eventName];
        }

        public static implicit operator string(SubscriptionClientEvent clientEvent)
        {
            return clientEvent.EventName;
        }
    }
}
