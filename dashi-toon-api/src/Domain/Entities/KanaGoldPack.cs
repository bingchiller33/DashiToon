namespace DashiToon.Api.Domain.Entities;

public class KanaGoldPack : BaseAuditableEntity<Guid>
{
    private KanaGoldPack()
    {
    }

    public int Amount { get; private set; }
    public Price Price { get; private set; } = null!;
    public bool IsActive { get; private set; }

    private static void ValidateAmount(int amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Số lượng phải lớn hơn 0");
        }
    }

    public static KanaGoldPack Create(int amount, Price price, bool isActive)
    {
        ValidateAmount(amount);

        return new KanaGoldPack
        {
            Id = Guid.NewGuid(),
            Amount = amount,
            Price = Price.CreateNew(price.Amount, price.Currency),
            IsActive = isActive
        };
    }

    public void Update(int amount, Price price, bool isActive)
    {
        ValidateAmount(amount);

        Amount = amount;
        Price = Price.CreateNew(price.Amount, price.Currency);
        IsActive = isActive;
    }

    public void UpdateStatus(bool isActive)
    {
        IsActive = isActive;
    }
}
