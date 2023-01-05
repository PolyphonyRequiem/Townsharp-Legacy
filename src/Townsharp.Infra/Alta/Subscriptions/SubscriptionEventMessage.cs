using System.Text.Json.Serialization;
using Townsharp.Infra.Alta.Json;

namespace Townsharp.Infra.Alta.Subscriptions
{
    internal abstract record SubscriptionEvent
    {
        protected SubscriptionEvent(int id, string @event, string key, long responseCode)
        {
            Id = id;
            Event = @event;
            Key = key;
            ResponseCode = responseCode;
        }

        internal int Id { get; init; }

        internal string Event { get; init; }

        internal string Key { get; init; }

        internal long ResponseCode { get; init; }
    }

    internal record GroupMemberUpdateMessage : SubscriptionEvent
    {
        internal GroupMemberUpdateMessage(int id, string key, long responseCode, ApiGroupMember content)
            : base(id, SubscriptionEventName.GroupMemberUpdate, key, responseCode)
        {
            Content = content;
        }

        internal ApiGroupMember Content { get; init; }
    }

    internal record ServerStatusMessage : SubscriptionEvent
    {
        internal ServerStatusMessage(int id, string key, long responseCode, ApiServer content)
            : base(id, SubscriptionEventName.GroupServerStatus, key, responseCode)
        {
            Content = content;
        }

        [JsonConverter(typeof(EmbeddedJsonConverter<ApiServer>))]
        internal ApiServer Content { get; init; }
    }

    internal record GroupUpdateMessage : SubscriptionEvent
    {
        internal GroupUpdateMessage(int id, string key, long responseCode, ApiGroup content)
            : base(id, SubscriptionEventName.GroupUpdate, key, responseCode)
        {
            Content = content;
        }

        internal ApiGroup Content { get; init; }
    }

    internal record MeGroupDeleteMessage : SubscriptionEvent
    {
        internal MeGroupDeleteMessage(int id, string key, long ResponseCode, (ApiGroup Group, ApiGroupMember Member) content)
            : base(id, SubscriptionEventName.MeGroupDelete, key, ResponseCode)
        {
            Content = content;
        }

        internal (ApiGroup Group, ApiGroupMember Member) Content { get; }
    }

    internal record MeGroupInviteCreateMessage : SubscriptionEvent
    {
        internal MeGroupInviteCreateMessage(int id, string key, long responseCode, ApiGroup content)
            : base(id, SubscriptionEventName.MeGroupInviteCreate, key, responseCode)
        {
            Content = content;
        }

        internal ApiGroup Content { get; }
    }

    internal record MeGroupInviteDeleteMessage : SubscriptionEvent
    {
        internal MeGroupInviteDeleteMessage(int id, string key, long responseCode, ApiGroup content)
            : base(id, SubscriptionEventName.MeGroupInviteDelete, key, responseCode)
        {
            Content = content;
        }

        internal ApiGroup Content { get; }
    }

    internal record MeGroupCreateMessage : SubscriptionEvent
    {
        internal MeGroupCreateMessage(int id, string key, long responseCode, ApiGroup content)
            : base(id, SubscriptionEventName.MeGroupCreate, key, responseCode)
        {
            Content = content;
        }

        internal ApiGroup Content { get; }
    }
}
