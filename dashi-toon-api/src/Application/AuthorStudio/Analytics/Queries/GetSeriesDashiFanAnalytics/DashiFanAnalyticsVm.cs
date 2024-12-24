using DashiToon.Api.Application.AuthorStudio.Analytics.Models;
using DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesDashiFanRankings;

namespace DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesDashiFanAnalytics;

public sealed record DashiFanAnalyticsVm(
    int Total,
    int? TotalCompare,
    double Growth,
    double? GrowthCompare,
    int Ranking,
    int? RankingCompare,
    IEnumerable<ChartData> Data,
    IEnumerable<DashiFanRanking> TopTiers);
