using DashiToon.Api.Application.Users.Queries.GetFollowedSeries;
using DashiToon.Api.Application.Users.Queries.GetSeriesFollowDetail;

namespace DashiToon.Api.Application.Common.Interfaces;

public interface IFollowRepository
{
    Task<(int Count, IEnumerable<FollowedSeriesDto> Result)> GetFollowedSeries(
        string userId,
        bool? hasRead,
        string sortBy,
        string sortOrder,
        int pageNumber,
        int pageSize);

    Task<FollowDetailDto?> GetFollowedSeriesById(string userId, int seriesId);
}
