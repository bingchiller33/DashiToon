using DashiToon.Api.Application.AuthorStudio.Analytics.Models;
using DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesDashiFanRankings;
using DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesGeneralAnalytis;
using DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesReviewAnalytics;
using DashiToon.Api.Application.Genres.Queries.GetGenres;

namespace DashiToon.Api.Application.Common.Interfaces;

public interface IAnalyticRepository
{
    Task<GeneralAnalyticsDto> GetGeneralAnalyticsAsync(DateRange range, int seriesId);
    Task<int> GetSeriesViewRankingAsync(DateRange range, int seriesId);
    Task<int> GetSeriesViewCountAsync(DateRange range, int seriesId);
    Task<IEnumerable<ChartDataDto>> GetSeriesViewCountBreakdownInDayAsync(DateRange range, int seriesId);
    Task<IEnumerable<ChartDataDto>> GetSeriesViewCountBreakdownInWeekAsync(DateRange range, int seriesId);

    Task<(int Count, IEnumerable<ChapterRankingsDto> Data)> GetSeriesChapterViewRankingAsync(
        DateRange range,
        int seriesId,
        string query = "",
        bool isDescending = false,
        int pageNumber = 1,
        int pageSize = 5);

    Task<int> GetSeriesCommentRankingAsync(DateRange range, int seriesId);
    Task<int> GetSeriesCommentCountAsync(DateRange range, int seriesId);
    Task<IEnumerable<ChartDataDto>> GetSeriesCommentCountBreakdownInDayAsync(DateRange range, int seriesId);

    Task<(int Count, IEnumerable<ChapterRankingsDto> Data)> GetSeriesChapterCommentRankingAsync(
        DateRange range,
        int seriesId,
        string query = "",
        bool isDescending = false,
        int pageNumber = 1,
        int pageSize = 5);

    Task<int> GetSeriesReviewRankingAsync(DateRange range, int seriesId);
    Task<(int ReviewCount, float Rating)> GetSeriesReviewCountAsync(DateRange range, int seriesId);
    Task<IEnumerable<ReviewChartDataDto>> GetSeriesReviewCountBreakdownInDayAsync(DateRange range, int seriesId);

    Task<int> GetSeriesKanaRevenueAsync(DateRange range, int seriesId);
    Task<int> GetSeriesKanaRevenueRankingAsync(DateRange range, int seriesId);
    Task<IEnumerable<ChartDataDto>> GetSeriesKanaRevenueBreakdownInDayAsync(DateRange range, int seriesId);

    Task<(int Count, IEnumerable<ChapterRankingsDto> Data)> GetSeriesChapterKanaRevenueRankingAsync(
        DateRange range,
        int seriesId,
        string query = "",
        bool isDescending = false,
        int pageNumber = 1,
        int pageSize = 5);

    Task<int> GetSeriesDashiFanSubscriberCountAsync(DateRange range, int seriesId);
    Task<int> GetSeriesDashiFanRankingAsync(DateRange range, int seriesId);
    Task<IEnumerable<ChartDataDto>> GetSeriesDashiFanBreakdownInMonthAsync(DateRange range, int seriesId);

    Task<(int Count, IEnumerable<DashiFanRankingDto> Data)> GetSeriesDashiFanTierRankingAsync(
        DateRange range,
        int seriesId,
        string query = "",
        bool isDescending = false,
        int pageNumber = 1,
        int pageSize = 5);

    Task<IEnumerable<int>> GetTopSeries(string interval);
    Task<IEnumerable<GenreDto>> GetTopGenres();
    Task<IEnumerable<int>> GetTopGenresSeries(int genreId);
}
