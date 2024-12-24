namespace DashiToon.Api.Domain.Entities;

public class KanaExchangeRate : BaseAuditableEntity<Guid>
{
    public string TargetCurrency { get; private set; } = null!;
    public decimal Rate { get; private set; }
    private static readonly List<string> SupportedCurrencies = ["VND"];

    private KanaExchangeRate()
    {
    }

    private static void ValidateRate(decimal rate)
    {
        if (rate <= 0)
        {
            throw new ArgumentException("Exchange rate must be greater than zero.", nameof(rate));
        }
    }

    private static void ValidateCurrency(string currency)
    {
        if (!SupportedCurrencies.Contains(currency))
        {
            throw new ArgumentException("Exchange currency not supported.", nameof(currency));
        }
    }

    public static KanaExchangeRate Create(string targetCurrency, decimal rate)
    {
        ValidateRate(rate);
        ValidateCurrency(targetCurrency);

        return new KanaExchangeRate { Id = Guid.NewGuid(), TargetCurrency = targetCurrency, Rate = rate };
    }

    public void UpdateRate(decimal rate)
    {
        ValidateRate(rate);

        Rate = rate;
    }
}
