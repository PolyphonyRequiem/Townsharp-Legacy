using Townsharp.Infra.MappingUtils;

namespace Townsharp.Infra.Alta.Subscriptions
{
    // NOTE! Consider SourceGenerators for this?
    public record SubscriptionEventName
    {
        public static string GroupMemberUpdate   = "group-member-update";
        public static string GroupServerCreate   = "group-server-create";
        public static string GroupServerDelete   = "group-server-delete";
        public static string GroupServerStatus   = "group-server-status";
        public static string GroupServerUpdate   = "group-server-update";
        public static string GroupUpdate         = "group-update";
        public static string MeGroupCreate       = "me-group-create";
        public static string MeGroupDelete       = "me-group-delete";
        public static string MeGroupInviteCreate = "me-group-invite-create";
        public static string MeGroupInviteDelete = "me-group-invite-delete";
    }
}
