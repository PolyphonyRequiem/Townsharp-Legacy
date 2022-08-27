using TownshipTale.Api.Core.Api.Schemas;

namespace TownshipTale.Api.Core
{
    public abstract record SubscriptionClientEventMessage
    {
        protected SubscriptionClientEventMessage(long id, SubscriptionClientEvent @event, string key, long responseCode)
        {
            Id = id;
            Event = @event;
            Key = key;
            ResponseCode = responseCode;
        }

        public long Id { get; init; }
        public SubscriptionClientEvent Event { get; init; }
        public string Key { get; init; }
        public long ResponseCode { get; init; }
    }

    public record GroupMemberUpdateMessage : SubscriptionClientEventMessage
    {
        public GroupMemberUpdateMessage(long id, string key, long responseCode, GroupMemberInfo content) : base(id, SubscriptionClientEvent.GroupMemberUpdate, key, responseCode)
        {
            this.Content = content;
        }

        public GroupMemberInfo Content { get; init; }
    }

    public record GroupServerStatusMessage : SubscriptionClientEventMessage
    {
        public GroupServerStatusMessage(long id, string key, long responseCode, ServerInfo content) : base(id, SubscriptionClientEvent.GroupServerStatus, key, responseCode)
        {
            Content = content;
        }

        public ServerInfo Content { get; init; }
    }

    public record GroupUpdateMessage : SubscriptionClientEventMessage
    {
        public GroupUpdateMessage(long id, string key, long responseCode, GroupInfo content) : base(id, SubscriptionClientEvent.GroupUpdate, key, responseCode)
        {
            Content = content;
        }

        public GroupInfo Content { get; init; }
    }

    public record MeGroupDeleteMessage : SubscriptionClientEventMessage
    {
        public MeGroupDeleteMessage(long id, string key, long ResponseCode, (GroupInfo Group, GroupMemberInfo Member) content) : base(id, SubscriptionClientEvent.MeGroupDelete, key, ResponseCode)
        {
            Content = content;
        }

        public (GroupInfo Group, GroupMemberInfo Member) Content { get; }
    }

    public record MeGroupInviteCreateMessage : SubscriptionClientEventMessage
    {
        public MeGroupInviteCreateMessage(long id, string key, long responseCode, GroupInfo content) : base(id, SubscriptionClientEvent.MeGroupInviteCreate, key, responseCode)
        {
            Content = content;
        }

        public GroupInfo Content { get; }
    }

    public record MeGroupInviteDeleteMessage : SubscriptionClientEventMessage
    {
        public MeGroupInviteDeleteMessage(long id, string key, long responseCode, GroupInfo content) : base(id, SubscriptionClientEvent.MeGroupInviteDelete, key, responseCode)
        {
            Content = content;
        }

        public GroupInfo Content { get; }
    }

    public record MeGroupCreateMessage : SubscriptionClientEventMessage
    {
        public MeGroupCreateMessage(long id, string key, long responseCode, GroupInfo content) : base(id, SubscriptionClientEvent.MeGroupCreate, key, responseCode)
        {
            Content = content;
        }

        public GroupInfo Content { get; }
    }
}
