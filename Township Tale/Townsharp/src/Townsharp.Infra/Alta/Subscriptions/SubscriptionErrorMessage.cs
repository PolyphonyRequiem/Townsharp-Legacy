namespace Townsharp.Infra.Alta.Subscriptions
{
    public record struct SubscriptionErrorMessage(long Id, string Key, long ResponseCode, SubscriptionErrorMessage.ClientMessageContent Content)
    {
        public const string Event = "response";

        public record struct ClientMessageContent(string Message, string ErrorCode);
    }
}
