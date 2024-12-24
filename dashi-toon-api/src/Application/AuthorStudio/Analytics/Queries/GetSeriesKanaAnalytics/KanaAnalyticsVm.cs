using DashiToon.Api.Application.AuthorStudio.Analytics.Models;

namespace DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesKanaAnalytics;

public sealed record KanaAnalyticsVm(
    int Total,
    int? TotalCompare,
    double Growth,
    double? GrowthCompare,
    int Ranking,
    int? RankingCompare,
    IEnumerable<ChartData> Data,
    IEnumerable<ChapterRankings> TopChapters);
