namespace Townsharp.Infra.Alta.Subscriptions
{
    public interface ISubscriptionEventHandler<in TEvent>
        where TEvent : SubscriptionEvent
    {
        Task Handle(TEvent @event, CancellationToken cancellationToken);
    }
}
