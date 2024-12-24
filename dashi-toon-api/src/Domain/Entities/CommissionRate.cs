namespace DashiToon.Api.Domain.Entities;

public class CommissionRate : BaseAuditableEntity<Guid>
{
    public CommissionType Type { get; private set; }
    public decimal RatePercentage { get; private set; }

    private CommissionRate()
    {
    }

    private static void ValidateRate(decimal rate)
    {
        if (rate is < 0 or > 100)
        {
            throw new ArgumentException("Rate percentage must be between 0 and 100.", nameof(rate));
        }
    }

    public static CommissionRate Create(CommissionType type, decimal ratePercentage)
    {
        ValidateRate(ratePercentage);

        return new CommissionRate { Id = Guid.NewGuid(), Type = type, RatePercentage = ratePercentage };
    }

    public void UpdateRate(decimal ratePercentage)
    {
        ValidateRate(ratePercentage);

        RatePercentage = ratePercentage;
    }
}

public enum CommissionType
{
    DashiFan,
    Kana
}
