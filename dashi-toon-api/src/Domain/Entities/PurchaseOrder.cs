namespace DashiToon.Api.Domain.Entities;

public class PurchaseOrder : BaseAuditableEntity<string>
{
    private PurchaseOrder()
    {
    }

    public string UserId { get; private set; } = null!;
    public Guid PackId { get; private set; }
    public Price Price { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public KanaGoldPack KanaGoldPack { get; private set; } = null!;

    public static PurchaseOrder CreateNew(
        string orderId,
        string userId,
        Guid packId,
        Price packPrice)
    {
        return new PurchaseOrder
        {
            Id = orderId,
            UserId = userId,
            PackId = packId,
            Price = Price.CreateNew(packPrice.Amount, packPrice.Currency),
            Status = OrderStatus.Pending
        };
    }

    public void CompleteOrder()
    {
        Status = OrderStatus.Success;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void CancelOrder()
    {
        Status = OrderStatus.Cancelled;
    }
}
