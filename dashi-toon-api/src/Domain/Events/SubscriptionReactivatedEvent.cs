namespace DashiToon.Api.Domain.Events;

public class SubscriptionReactivatedEvent : BaseEvent
{
    public SubscriptionReactivatedEvent(Subscription subscription, string reason)
    {
        Subscription = subscription;
        Reason = reason;
    }

    public Subscription Subscription { get; }
    public string Reason { get; }
}
