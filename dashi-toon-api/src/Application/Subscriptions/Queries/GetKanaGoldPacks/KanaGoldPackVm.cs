using DashiToon.Api.Domain.ValueObjects;

namespace DashiToon.Api.Application.Subscriptions.Queries.GetKanaGoldPacks;

public record struct KanaGoldPackVm(
    Guid Id,
    int Amount,
    Price Price
);
