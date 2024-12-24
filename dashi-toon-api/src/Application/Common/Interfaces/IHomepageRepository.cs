using DashiToon.Api.Application.Homepage.Models;
using DashiToon.Api.Application.Homepage.Queries.GetRecentlyUpdatedSeries;
using DashiToon.Api.Application.Homepage.Queries.GetRecommendSeries;

namespace DashiToon.Api.Application.Common.Interfaces;

public interface IHomepageRepository
{
    Task<IEnumerable<RecentlyUpdatedSeriesDto>> GetRecentlyUpdatedSeries();
    Task<IEnumerable<SeriesInfoDto>> GetSeries(int[] seriesIds, int pageNumber = 1, int pageSize = 12);
    Task<IEnumerable<SeriesInfoDto>> GetRecentlyReleasedSeries();
    Task<IEnumerable<UserLikedSeriesDto>> GetUserLikedSeries();
}
