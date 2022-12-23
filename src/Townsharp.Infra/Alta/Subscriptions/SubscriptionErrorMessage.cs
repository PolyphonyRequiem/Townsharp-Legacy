namespace Townsharp.Infra.Alta.Subscriptions
{
    internal record struct SubscriptionErrorMessage(long Id, string Key, long ResponseCode, SubscriptionErrorMessage.ClientMessageContent Content)
    {
        internal const string Event = "response";

        internal record struct ClientMessageContent(string Message, string ErrorCode);
    }
}
