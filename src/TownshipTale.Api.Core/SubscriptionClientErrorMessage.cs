namespace TownshipTale.Api.Core
{
    public record struct SubscriptionClientErrorMessage(long Id, string Key, long ResponseCode, SubscriptionClientErrorMessage.ClientMessageContent Content)
    {
        public const string Event = "response";

        public record struct ClientMessageContent(string Message, string ErrorCode);
    }
}
