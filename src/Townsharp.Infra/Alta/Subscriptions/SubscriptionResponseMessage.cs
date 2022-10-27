﻿namespace Townsharp.Infra.Alta.Subscriptions
{
    public record SubscriptionResponseMessage<T>
    {
        protected SubscriptionResponseMessage(long id, string @event, long responseCode, T content)
        {
            Id = id;
            Event = @event;
            ResponseCode = responseCode;
            Content = content;
        }

        public long Id { get; init; }
        public string Event { get; init; }
        public long ResponseCode { get; init; }
        public T Content { get; init; }
    }

    public record DeleteSubscriptionResponseMessage : SubscriptionResponseMessage<string>
    {
        public DeleteSubscriptionResponseMessage(long Id, long ResponseCode, SubscriptionEventName @event) : base(Id, SubscriptionEventName.GroupMemberUpdate, ResponseCode, string.Empty)
        {
            Key = $"DELETE /ws/subscription/({@event})";
        }

        public string Key { get; init; }
    }

    public record struct MigrateSubscriptionContent(string Token);

    public record GetMigrateResponseMessage : SubscriptionResponseMessage<MigrateSubscriptionContent>
    {
        public GetMigrateResponseMessage(long Id, long ResponseCode, SubscriptionEventName @event, MigrateSubscriptionContent content) : base(Id, SubscriptionEventName.GroupMemberUpdate, ResponseCode, content)
        {
            Key = $"GET /ws/migrate";
        }

        public string Key { get; init; }
    }

    public record PostMigrateResponseMessage : SubscriptionResponseMessage<string>
    {
        public PostMigrateResponseMessage(long Id, long ResponseCode, SubscriptionEventName @event) : base(Id, SubscriptionEventName.GroupMemberUpdate, ResponseCode, string.Empty)
        {
            Key = $"POST /ws/migrate";
        }

        public string Key { get; init; }
    }

    public record PostSubscriptionResponseMessage : SubscriptionResponseMessage<string>
    {
        public PostSubscriptionResponseMessage(long Id, long ResponseCode, SubscriptionEventName @event) : base(Id, SubscriptionEventName.GroupMemberUpdate, ResponseCode, string.Empty)
        {
            Key = $"POST /ws/subscription/{@event}";
        }

        public string Key { get; init; }
    }
}
