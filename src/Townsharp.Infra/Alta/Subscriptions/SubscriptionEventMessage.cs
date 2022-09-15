using System.Text.Json.Serialization;
using Townsharp.Groups;
using Townsharp.Infra.Alta.Api;
using Townsharp.Infra.Alta.Json;

namespace Townsharp.Infra.Alta.Subscriptions
{
    public abstract record SubscriptionEvent
    {
        protected SubscriptionEvent(int id, string @event, string key, long responseCode)
        {
            Id = id;
            Event = @event;
            Key = key;
            ResponseCode = responseCode;
        }

        public int Id { get; init; }

        public string Event { get; init; }

        public string Key { get; init; }

        public long ResponseCode { get; init; }
    }

    public record GroupMemberUpdateMessage : SubscriptionEvent
    {
        public GroupMemberUpdateMessage(int id, string key, long responseCode, GroupMemberInfo content)
            : base(id, SubscriptionEventName.GroupMemberUpdate, key, responseCode)
        {
            Content = content;
        }

        public GroupMemberInfo Content { get; init; }
    }

    public record GroupServerStatusMessage : SubscriptionEvent
    {
        public GroupServerStatusMessage(int id, string key, long responseCode, ServerInfo content)
            : base(id, SubscriptionEventName.GroupServerStatus, key, responseCode)
        {
            Content = content;
        }

        [JsonConverter(typeof(EmbeddedJsonConverter<ServerInfo>))]
        public ServerInfo Content { get; init; }
    }

    public record GroupUpdateMessage : SubscriptionEvent
    {
        public GroupUpdateMessage(int id, string key, long responseCode, GroupInfo content)
            : base(id, SubscriptionEventName.GroupUpdate, key, responseCode)
        {
            Content = content;
        }

        public GroupInfo Content { get; init; }
    }

    public record MeGroupDeleteMessage : SubscriptionEvent
    {
        public MeGroupDeleteMessage(int id, string key, long ResponseCode, (GroupInfo Group, GroupMemberInfo Member) content)
            : base(id, SubscriptionEventName.MeGroupDelete, key, ResponseCode)
        {
            Content = content;
        }

        public (GroupInfo Group, GroupMemberInfo Member) Content { get; }
    }

    public record MeGroupInviteCreateMessage : SubscriptionEvent
    {
        public MeGroupInviteCreateMessage(int id, string key, long responseCode, GroupInfo content)
            : base(id, SubscriptionEventName.MeGroupInviteCreate, key, responseCode)
        {
            Content = content;
        }

        public GroupInfo Content { get; }
    }

    public record MeGroupInviteDeleteMessage : SubscriptionEvent
    {
        public MeGroupInviteDeleteMessage(int id, string key, long responseCode, GroupInfo content)
            : base(id, SubscriptionEventName.MeGroupInviteDelete, key, responseCode)
        {
            Content = content;
        }

        public GroupInfo Content { get; }
    }

    public record MeGroupCreateMessage : SubscriptionEvent
    {
        public MeGroupCreateMessage(int id, string key, long responseCode, GroupInfo content)
            : base(id, SubscriptionEventName.MeGroupCreate, key, responseCode)
        {
            Content = content;
        }

        public GroupInfo Content { get; }
    }
}
