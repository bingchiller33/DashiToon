namespace DashiToon.Api.Domain.ValueObjects;

public class Price : ValueObject
{
    private static readonly List<string> AllowedCurrencies = ["USD", "VND"];

    private static readonly Dictionary<string, decimal> MinimumPriceMap = new() { { "USD", 0.1m }, { "VND", 10_000m } };

    private static readonly Dictionary<string, decimal> MaximumPriceMap =
        new() { { "USD", 10_000m }, { "VND", 100_000_000m } };

    private Price()
    {
    }

    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;

    public static Price CreateNew(decimal amount, string currency)
    {
        ValidatePrice(amount, currency);

        return new Price { Amount = amount, Currency = currency };
    }

    private static void ValidatePrice(decimal amount, string currency)
    {
        if (!AllowedCurrencies.Contains(currency))
        {
            throw new ArgumentException($"Currency '{currency}' is not supported.");
        }

        if (amount < MinimumPriceMap[currency])
        {
            throw new ArgumentException($"Amount cannot be less than {MinimumPriceMap[currency]}");
        }

        if (amount > MaximumPriceMap[currency])
        {
            throw new ArgumentException($"Amount cannot be more than {MaximumPriceMap[currency]}");
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
