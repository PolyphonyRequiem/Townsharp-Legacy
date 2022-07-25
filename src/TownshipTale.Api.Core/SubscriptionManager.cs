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

        internal Task Subscribe<T>(SubscriptionClientEvent meGroupInviteCreate, string key, Action<SubscriptionClientEventMessage<T>> messageHandler)
        { 
            throw new NotImplementedException();
        }
    }
}
