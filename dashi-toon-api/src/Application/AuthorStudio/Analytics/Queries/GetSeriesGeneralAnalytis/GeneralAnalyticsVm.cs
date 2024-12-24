namespace DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesGeneralAnalytis;

public sealed record GeneralAnalyticsVm(
    int CurrentView,
    int? CompareView,
    float CurrentRating,
    float? CompareRating,
    double CurrentRevenue,
    double? CompareRevenue,
    int CurrentDashiFan,
    double? CompareDashiFan
);

public sealed class GeneralAnalyticsDto
{
    public float Rating { get; init; }
    public int ViewCount { get; set; }
    public double Revenue { get; set; }
    public int DashiFanCount { get; set; }
}
