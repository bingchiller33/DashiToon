namespace DashiToon.Api.Domain.Events;

public class SubscriptionCancelledEvent : BaseEvent
{
    public SubscriptionCancelledEvent(Subscription subscription, string reason)
    {
        Subscription = subscription;
        Reason = reason;
    }

    public Subscription Subscription { get; }
    public string Reason { get; }
}
