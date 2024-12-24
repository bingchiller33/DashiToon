using DashiToon.Api.Application.Reviews.Models;

namespace DashiToon.Api.Application.Common.Interfaces;

public interface IReviewRepository
{
    Task<ReviewDto?> FindReviewByUserAndSeries(int seriesId, string userId);

    Task<(int Count, IEnumerable<ReviewDto> Result)> FindReviews(
        int seriesId,
        int pageNumber,
        int pageSize,
        string sortBy);
}
