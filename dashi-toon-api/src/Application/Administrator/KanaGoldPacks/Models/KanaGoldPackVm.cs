using DashiToon.Api.Domain.ValueObjects;

namespace DashiToon.Api.Application.Administrator.KanaGoldPacks.Models;

public sealed record KanaGoldPackVm(
    Guid Id,
    int Amount,
    Price Price,
    bool IsActive
);
