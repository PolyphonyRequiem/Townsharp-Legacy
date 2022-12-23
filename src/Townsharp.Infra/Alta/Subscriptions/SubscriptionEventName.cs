using Townsharp.Infra.MappingUtils;

namespace Townsharp.Infra.Alta.Subscriptions
{
    // NOTE! Consider SourceGenerators for this?
    internal record SubscriptionEventName
    {
        internal static string GroupMemberUpdate   = "group-member-update";
        internal static string GroupServerCreate   = "group-server-create";
        internal static string GroupServerDelete   = "group-server-delete";
        internal static string GroupServerStatus   = "group-server-status";
        internal static string GroupServerUpdate   = "group-server-update";
        internal static string GroupUpdate         = "group-update";
        internal static string MeGroupCreate       = "me-group-create";
        internal static string MeGroupDelete       = "me-group-delete";
        internal static string MeGroupInviteCreate = "me-group-invite-create";
        internal static string MeGroupInviteDelete = "me-group-invite-delete";
    }
}
