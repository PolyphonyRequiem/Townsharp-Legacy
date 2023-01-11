namespace Townsharp.Subscriptions
{
    public record SubscriptionRequestResult
    {
        private SubscriptionRequestResult()
        {
            ErrorMessage = String.Empty;
            IsSuccess = true;
        }

        private SubscriptionRequestResult(string errorMessage)
        {
            ErrorMessage = errorMessage;
            IsSuccess = false;
        }

        public static SubscriptionRequestResult Success()
        {
            return new SubscriptionRequestResult();
        }

        public static SubscriptionRequestResult Fail(string errorMessage)
        {
            return new SubscriptionRequestResult(errorMessage);
        }

        public bool IsSuccess { get; }

        public string ErrorMessage { get; }
    }
}