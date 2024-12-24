using DashiToon.Api.Domain.ValueObjects;

namespace DashiToon.Api.Application.Common.Interfaces;

public interface ICurrencyService
{
    Price ConvertPrice(Price price, string targetCurrency);
}
