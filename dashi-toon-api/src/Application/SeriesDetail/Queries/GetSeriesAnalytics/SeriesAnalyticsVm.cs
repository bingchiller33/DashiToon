namespace DashiToon.Api.Application.SeriesDetail.Queries.GetSeriesAnalytics;

public sealed record SeriesAnalyticsVm(
    double Rating,
    int ReviewCount,
    int FollowCount,
    int ViewCount,
    string LastModified
);

public record struct SeriesAnalyticsDto(
    int RecommendedCount,
    int ReviewCount,
    int FollowCount,
    int ViewCount,
    DateTimeOffset LastModified
);
