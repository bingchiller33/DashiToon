namespace DashiToon.Api.Domain.Entities;

public class Subscription : BaseAuditableEntity<Guid>
{
    private readonly List<SubscriptionHistory> _histories = [];

    private Subscription()
    {
    }

    public string SubscriptionId { get; private set; } = null!;
    public string UserId { get; private set; } = null!;
    public Guid DashiFanId { get; private set; }
    public DateTimeOffset? NextBillingDate { get; private set; }
    public SubscriptionStatus Status { get; set; }
    public DashiFan Tier { get; private set; } = null!;
    public IReadOnlyList<SubscriptionHistory> Histories => _histories.AsReadOnly();

    internal static Subscription Create(
        string userId,
        Guid dashiFanId)
    {
        Subscription subscription = new()
        {
            Id = Guid.NewGuid(), UserId = userId, DashiFanId = dashiFanId, Status = SubscriptionStatus.Pending
        };

        return subscription;
    }

    public void CompleteCreation(string subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }

    public void ChangeTier(DashiFan tier)
    {
        DashiFanId = tier.Id;

        AddDomainEvent(new SubscriptionTierUpdatedEvent(this));
    }

    public void Activate(DateTimeOffset? nextBillingDate)
    {
        Status = SubscriptionStatus.Active;
        NextBillingDate = nextBillingDate;

        _histories.Add(SubscriptionHistory.Create(Status));
    }

    public void Cancel()
    {
        SubscriptionStatus previousStatus = Status;

        Status = SubscriptionStatus.Cancelled;

        if (previousStatus != SubscriptionStatus.Pending)
        {
            AddDomainEvent(new SubscriptionCancelledEvent(this, "Subscription Cancelled"));
        }

        _histories.Add(SubscriptionHistory.Create(Status));
    }

    public void Expired()
    {
        Status = SubscriptionStatus.Expired;
        _histories.Add(SubscriptionHistory.Create(Status));
    }

    public void Suspend()
    {
        Status = SubscriptionStatus.Suspended;

        AddDomainEvent(new SubscriptionSuspendedEvent(this, "Subscription Suspended"));
        _histories.Add(SubscriptionHistory.Create(Status));
    }

    public void Reactivate()
    {
        Status = SubscriptionStatus.Active;

        AddDomainEvent(new SubscriptionReactivatedEvent(this, "Subscription Reactivated"));
        _histories.Add(SubscriptionHistory.Create(Status));
    }
}

public class SubscriptionHistory
{
    public SubscriptionStatus Status { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }

    private SubscriptionHistory() { }

    internal static SubscriptionHistory Create(SubscriptionStatus status)
    {
        return new SubscriptionHistory { Status = status, Timestamp = DateTimeOffset.UtcNow };
    }
}
