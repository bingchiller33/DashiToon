namespace DashiToon.Api.Application.Administrator.SystemSettings.Models;

public sealed record KanaExchangeRateVm(
    string Currency,
    decimal Rate
);
