using DashiToon.Api.Application.AuthorStudio.Analytics.Models;

namespace DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesViewAnalytics;

public sealed record ViewAnalyticsVm(
    int Total,
    int? TotalCompare,
    double Growth,
    double? GrowthCompare,
    int Ranking,
    int? RankingCompare,
    IEnumerable<ChartData> Data,
    IEnumerable<ChapterRankings> TopChapters,
    string BestDayOfWeek,
    string WorstDayOfWeek,
    IEnumerable<ChartData> DayOfWeeks);
