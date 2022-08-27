namespace TownshipTale.Api.Core
{
    internal class SubscriptionManager
    {
        private ApiClient apiClient;

        internal SubscriptionManager(ApiClient apiClient)
        {
            this.apiClient = apiClient;
        }

        internal Task InitializeSubscriptionsAsync()
        {
            throw new NotImplementedException();
        }

        internal Task Subscribe<T>(SubscriptionClientEvent @event, string key, Action<T> messageHandler) where T: SubscriptionClientEventMessage
        { 
            throw new NotImplementedException();
        }
    }
}
