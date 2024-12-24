using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Administrator.SystemSettings.Models;

public sealed record CommissionRateVm(
    CommissionType Type,
    decimal Rate
);
