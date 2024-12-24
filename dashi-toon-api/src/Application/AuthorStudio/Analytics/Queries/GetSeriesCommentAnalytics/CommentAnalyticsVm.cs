using DashiToon.Api.Application.AuthorStudio.Analytics.Models;
using DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesViewAnalytics;

namespace DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesCommentAnalytics;

public sealed record CommentAnalyticsVm(
    int Total,
    int? TotalCompare,
    double Growth,
    double? GrowthCompare,
    int Ranking,
    int? RankingCompare,
    IEnumerable<ChartData> Data,
    IEnumerable<ChapterRankings> TopChapters);
