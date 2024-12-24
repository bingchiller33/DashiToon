using DashiToon.Api.Application.Users.Queries.GetSubscribedSeries;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.Users.Queries.GetSeriesSubscription;

public sealed record SeriesSubscriptionVm(
    Guid SubscriptionId,
    SubscriptionDashiFan Tier,
    SubscriptionStatus Status,
    string? NextBillingDate
);
