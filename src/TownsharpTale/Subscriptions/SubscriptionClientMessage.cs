namespace Townsharp.Subscriptions
{
    internal record SubscriptionClientMessage(long id, HttpMethod Method, string Path, string Content = "");
}
