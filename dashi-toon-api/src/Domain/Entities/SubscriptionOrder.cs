namespace DashiToon.Api.Domain.Entities;

public class SubscriptionOrder : BaseAuditableEntity<string>
{
    private SubscriptionOrder() { }
    public string UserId { get; private set; } = null!;
    public Guid SubscriptionId { get; private set; }
    public Price Price { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public Guid? UpgradeTierId { get; private set; }
    public Subscription Subscription { get; private set; } = null!;

    public static SubscriptionOrder CreateNew(
        string orderId,
        string userId,
        Guid subscriptionId,
        Price subscriptionPrice)
    {
        return new SubscriptionOrder
        {
            Id = orderId,
            UserId = userId,
            SubscriptionId = subscriptionId,
            Price = Price.CreateNew(subscriptionPrice.Amount, subscriptionPrice.Currency),
            Status = OrderStatus.Pending
        };
    }

    public void CompleteOrder()
    {
        CompletedAt = DateTimeOffset.UtcNow;
        Status = OrderStatus.Success;
    }

    public static SubscriptionOrder CreateUpgradeOrder(
        string orderId,
        string userId,
        Guid subscriptionId,
        Guid upgradeTierId,
        Price upgradePrice)
    {
        return new SubscriptionOrder
        {
            Id = orderId,
            UserId = userId,
            SubscriptionId = subscriptionId,
            Status = OrderStatus.Pending,
            Price = Price.CreateNew(upgradePrice.Amount, upgradePrice.Currency),
            UpgradeTierId = upgradeTierId
        };
    }

    public void CancelOrder()
    {
        Status = OrderStatus.Cancelled;
    }
}
