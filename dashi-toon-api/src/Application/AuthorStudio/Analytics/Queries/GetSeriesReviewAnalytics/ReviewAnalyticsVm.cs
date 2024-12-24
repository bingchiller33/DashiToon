namespace DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesReviewAnalytics;

public sealed record ReviewAnalyticsVm(
    int Total,
    int? TotalCompare,
    float Positive,
    float? PositiveCompare,
    int Ranking,
    int? RankingCompare,
    IEnumerable<ReviewChartData> Data);

public sealed record ReviewChartData(
    string Time,
    int CurrentPositive,
    int CurrentNegative,
    int? ComparePositive,
    int? CompareNegative
);

public sealed class ReviewChartDataDto
{
    public required string Time { get; init; }
    public int Positive { get; init; }
    public int Negative { get; init; }
}
