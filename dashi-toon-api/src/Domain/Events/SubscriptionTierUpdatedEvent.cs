namespace DashiToon.Api.Domain.Events;

public class SubscriptionTierUpdatedEvent : BaseEvent
{
    public SubscriptionTierUpdatedEvent(Subscription subscription)
    {
        Subscription = subscription;
    }

    public Subscription Subscription { get; }
}
