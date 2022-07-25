namespace TownshipTale.Api.Core
{
    public record SubscriptionClientResponseMessage<T>
    {
        protected SubscriptionClientResponseMessage(long id, SubscriptionClientEvent @event, long responseCode, T content)
        {
            Id = id;
            Event = @event;
            ResponseCode = responseCode;
            Content = content;
        }

        public long Id { get; init; }
        public SubscriptionClientEvent Event { get; init; }
        public long ResponseCode { get; init; }
        public T Content { get; init; }
    }

    public record DeleteSubscriptionResponseMessage : SubscriptionClientResponseMessage<string>
    {
        public DeleteSubscriptionResponseMessage(long Id, long ResponseCode, SubscriptionClientEvent @event) : base(Id, SubscriptionClientEvent.GroupMemberUpdate, ResponseCode, string.Empty)
        {
            Key = $"DELETE /ws/subscription/({@event})";
        }

        public string Key { get; init; }
    }

    public record struct MigrateSubscriptionContent(string Token);

    public record GetMigrateResponseMessage : SubscriptionClientResponseMessage<MigrateSubscriptionContent>
    {
        public GetMigrateResponseMessage(long Id, long ResponseCode, SubscriptionClientEvent @event, MigrateSubscriptionContent content) : base(Id, SubscriptionClientEvent.GroupMemberUpdate, ResponseCode, content)
        {
            Key = $"GET /ws/migrate";
        }

        public string Key { get; init; }
    }

    public record PostMigrateResponseMessage : SubscriptionClientResponseMessage<string>
    {
        public PostMigrateResponseMessage(long Id, long ResponseCode, SubscriptionClientEvent @event) : base(Id, SubscriptionClientEvent.GroupMemberUpdate, ResponseCode, string.Empty)
        {
            Key = $"POST /ws/migrate";
        }

        public string Key { get; init; }
    }

    public record PostSubscriptionResponseMessage : SubscriptionClientResponseMessage<string>
    {
        public PostSubscriptionResponseMessage(long Id, long ResponseCode, SubscriptionClientEvent @event) : base(Id, SubscriptionClientEvent.GroupMemberUpdate, ResponseCode, string.Empty)
        {
            Key = $"POST /ws/subscription/{@event}";
        }

        public string Key { get; init; }
    }
}
