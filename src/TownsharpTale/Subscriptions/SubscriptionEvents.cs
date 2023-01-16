using Townsharp.Groups;
using Townsharp.Servers;

namespace Townsharp.Subscriptions
{
    internal static class SubscriptionEventNames
    {
        internal static string GroupMemberUpdate = "group-member-update";
        internal static string GroupServerCreate = "group-server-create";
        internal static string GroupServerDelete = "group-server-delete";
        internal static string GroupServerStatus = "group-server-status";
        internal static string GroupServerUpdate = "group-server-update";
        internal static string GroupUpdate = "group-update";
        internal static string MeGroupCreate = "me-group-create";
        internal static string MeGroupDelete = "me-group-delete";
        internal static string MeGroupInviteCreate = "me-group-invite-create";
        internal static string MeGroupInviteDelete = "me-group-invite-delete";
    }

    public interface ISubscriptionEvent
    {
        internal string Event { get; }
    }

    // Hey buddy?  Most of these don't matter :) move on!

    public class GroupMemberUpdateEvent : ISubscriptionEvent
    {
        public GroupId Id { get; }

        public GroupMemberInfo Member { get; }

        string ISubscriptionEvent.Event => SubscriptionEventNames.GroupMemberUpdate;

        public GroupMemberUpdateEvent(GroupId groupId, GroupMemberInfo groupMember)
        {
            this.Id = groupId;
            this.Member = groupMember;
        }
    }

    public class ServerStatusUpdateEvent : ISubscriptionEvent
    {
        public GroupId Id { get; }

        public ServerStatus ServerStatus { get; }

        string ISubscriptionEvent.Event => SubscriptionEventNames.GroupServerStatus;

        public ServerStatusUpdateEvent(GroupId groupId, ServerStatus serverStatus)
        {
            this.Id = groupId;
            this.ServerStatus = serverStatus;
        }
    }

    public class GroupUpdateEvent : ISubscriptionEvent 
    {
        public GroupId Id { get; }

        public GroupInfo GroupInfo { get; }

        string ISubscriptionEvent.Event => SubscriptionEventNames.GroupUpdate;

        public GroupUpdateEvent(GroupId groupId, GroupInfo groupInfo)
        {
            this.Id = groupId;
            this.GroupInfo = groupInfo;
        }
    }

    public class KickedFromGroupEvent : ISubscriptionEvent
    {
        public GroupId Id { get; }

        string ISubscriptionEvent.Event => SubscriptionEventNames.MeGroupDelete;

        public KickedFromGroupEvent(GroupId groupId)
        {
            this.Id = groupId;
        }
    }

    public static class ISubscriptionEventMessageExtensions
    {

    }
}
