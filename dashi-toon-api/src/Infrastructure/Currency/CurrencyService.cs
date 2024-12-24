using System.Text.Json;
using System.Text.Json.Serialization;
using currencyapi;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.ValueObjects;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DashiToon.Api.Infrastructure.Currency;

public class CurrencyService : ICurrencyService
{
    private readonly Currencyapi _currencyapi;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CurrencyService> _logger;
    private const int CacheExpirationInDays = 1;

    public CurrencyService(Currencyapi currencyapi, IMemoryCache memoryCache, ILogger<CurrencyService> logger)
    {
        _currencyapi = currencyapi;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public Price ConvertPrice(Price price, string targetCurrency)
    {
        if (!_memoryCache.TryGetValue<Dictionary<string, decimal>>(price.Currency,
                out Dictionary<string, decimal>? currencyRates))
        {
            currencyRates = GetCurrencyRates(price.Currency);
            if (currencyRates == null)
            {
                _logger.LogError("Failed to get exchange rates for {Currency}.", price.Currency);
                return price; // If rates couldn't be retrieved, return the original price
            }

            _memoryCache.Set(price.Currency, currencyRates, TimeSpan.FromDays(CacheExpirationInDays));
        }

        if (currencyRates!.TryGetValue(targetCurrency, out decimal rate))
        {
            return Price.CreateNew(rate * price.Amount, targetCurrency);
        }

        ExchangeRatesResponse? response = JsonSerializer.Deserialize<ExchangeRatesResponse>(_currencyapi.Latest(
            price.Currency,
            targetCurrency));

        if (response?.Data == null || !response.Data.TryGetValue(targetCurrency, out CurrencyRate? targetRate))
        {
            _logger.LogWarning("Rate for target currency {Currency} not found.", targetCurrency);
            return price; // If the specific target currency rate is not found, return the original price
        }

        currencyRates[targetCurrency] = targetRate.Value;

        return Price.CreateNew(targetRate.Value * price.Amount, targetCurrency);
    }

    private Dictionary<string, decimal>? GetCurrencyRates(string baseCurrency)
    {
        ExchangeRatesResponse? response =
            JsonSerializer.Deserialize<ExchangeRatesResponse>(_currencyapi.Latest(baseCurrency));
        return response?.Data.ToDictionary(x => x.Key, x => x.Value.Value);
    }
}

public class ExchangeRatesResponse
{
    [JsonPropertyName("data")] public Dictionary<string, CurrencyRate> Data { get; init; } = [];
}

public class CurrencyRate
{
    [JsonPropertyName("code")] public string Code { get; set; } = null!;
    [JsonPropertyName("value")] public decimal Value { get; set; }
}
