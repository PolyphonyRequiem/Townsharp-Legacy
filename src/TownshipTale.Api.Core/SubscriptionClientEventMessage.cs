using TownshipTale.Api.Core.Api.Schemas;

namespace TownshipTale.Api.Core
{
    public record SubscriptionClientEventMessage<T>
    {
        protected SubscriptionClientEventMessage(long id, SubscriptionClientEvent @event, string key, long responseCode, T content)
        {
            Id = id;
            Event = @event;
            Key = key;
            ResponseCode = responseCode;
            Content = content;
        }

        public long Id { get; init; }
        public SubscriptionClientEvent Event { get; init; }
        public string Key { get; init; }
        public long ResponseCode { get; init; }
        public T Content { get; init; }
    }

    public record GroupMemberUpdateMessage : SubscriptionClientEventMessage<GroupMemberInfo>
    {
        public GroupMemberUpdateMessage(long Id, string Key, long ResponseCode, GroupMemberInfo Content) : base(Id, SubscriptionClientEvent.GroupMemberUpdate, Key, ResponseCode, Content)
        {
        }
    }

    public record GroupServerStatusMessage : SubscriptionClientEventMessage<ServerInfo>
    {
        public GroupServerStatusMessage(long Id, string Key, long ResponseCode, ServerInfo Content) : base(Id, SubscriptionClientEvent.GroupServerStatus, Key, ResponseCode, Content)
        {
        }
    }

    public record GroupUpdateMessage : SubscriptionClientEventMessage<GroupInfo>
    {
        public GroupUpdateMessage(long Id, string Key, long ResponseCode, GroupInfo Content) : base(Id, SubscriptionClientEvent.GroupUpdate, Key, ResponseCode, Content)
        {
        }
    }

    public record MeGroupDeleteMessage : SubscriptionClientEventMessage<(GroupInfo, GroupMemberInfo)>
    {
        public MeGroupDeleteMessage(long Id, string Key, long ResponseCode, (GroupInfo, GroupMemberInfo) Content) : base(Id, SubscriptionClientEvent.MeGroupDelete, Key, ResponseCode, Content)
        {
        }
    }

    public record MeGroupInviteCreateMessage : SubscriptionClientEventMessage<GroupInfo>
    {
        public MeGroupInviteCreateMessage(long Id, string Key, long ResponseCode, GroupInfo Content) : base(Id, SubscriptionClientEvent.MeGroupInviteCreate, Key, ResponseCode, Content)
        {
        }
    }
    public record MeGroupInviteDeleteMessage : SubscriptionClientEventMessage<GroupInfo>
    {
        public MeGroupInviteDeleteMessage(long Id, SubscriptionClientEvent Event, string Key, long ResponseCode, GroupInfo Content) : base(Id, SubscriptionClientEvent.MeGroupInviteDelete, Key, ResponseCode, Content)
        {
        }
    }

    public record MeGroupCreateMessage : SubscriptionClientEventMessage<GroupInfo>
    {
        public MeGroupCreateMessage(long Id, SubscriptionClientEvent Event, string Key, long ResponseCode, GroupInfo Content) : base(Id, SubscriptionClientEvent.MeGroupCreate, Key, ResponseCode, Content)
        {
        }
    }
}
