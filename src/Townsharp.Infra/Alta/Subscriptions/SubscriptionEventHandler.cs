namespace Townsharp.Infra.Alta.Subscriptions
{
    public abstract class SubscriptionEventHandler<TEvent> : ISubscriptionEventHandler<TEvent>
        where TEvent : SubscriptionEvent
    {
        Task ISubscriptionEventHandler<TEvent>.Handle(TEvent @event, CancellationToken cancellationToken)            
        {
            Handle(@event);

            return Task.CompletedTask;
        }

        protected abstract void Handle(TEvent @event);
    }
}
