namespace DashiToon.Api.Domain.Events;

public class SubscriptionSuspendedEvent : BaseEvent
{
    public SubscriptionSuspendedEvent(Subscription subscription, string reason)
    {
        Subscription = subscription;
        Reason = reason;
    }

    public Subscription Subscription { get; }
    public string Reason { get; }
}
