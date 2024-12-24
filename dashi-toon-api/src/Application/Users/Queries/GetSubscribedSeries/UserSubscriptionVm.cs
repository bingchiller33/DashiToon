using DashiToon.Api.Domain.Enums;
using DashiToon.Api.Domain.ValueObjects;

namespace DashiToon.Api.Application.Users.Queries.GetSubscribedSeries;

public sealed record UserSubscriptionVm(
    Guid SubscriptionId,
    SubscriptionSeries Series,
    SubscriptionDashiFan DashiFan,
    string NextBillingDate,
    string SubscribedSince,
    SubscriptionStatus Status
);

public sealed record SubscriptionSeries(
    int? Id,
    string? ThumbnailUrl,
    string? Title,
    string? Author,
    string? LastModified
);

public sealed record SubscriptionDashiFan(
    Guid? Id,
    string? Name,
    string? Description,
    int? Perks,
    Price? Price
);
