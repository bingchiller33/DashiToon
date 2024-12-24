using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Reviews.Models;

namespace DashiToon.Api.Application.Reviews.Queries.GetSeriesReviews;

public sealed record GetSeriesReviewQuery(
    int SeriesId,
    int PageNumber = 1,
    int PageSize = 10,
    string SortBy = "Relevance") : IRequest<PaginatedList<ReviewVm>>;

public sealed class GetSeriesReviewQueryHandler : IRequestHandler<GetSeriesReviewQuery, PaginatedList<ReviewVm>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IImageStore _imageStore;

    public GetSeriesReviewQueryHandler(
        IReviewRepository reviewRepository,
        IImageStore imageStore)
    {
        _reviewRepository = reviewRepository;
        _imageStore = imageStore;
    }

    public async Task<PaginatedList<ReviewVm>> Handle(
        GetSeriesReviewQuery request,
        CancellationToken cancellationToken)
    {
        (int count, IEnumerable<ReviewDto> reviews) = await _reviewRepository.FindReviews(
            request.SeriesId,
            request.PageNumber,
            request.PageSize,
            request.SortBy);

        List<ReviewVm>? result = new(request.PageSize);

        foreach (ReviewDto? review in reviews)
        {
            result.Add(new ReviewVm(
                review.Id,
                review.Content,
                review.IsRecommended,
                review.Likes,
                review.Dislikes,
                review.UserId,
                review.UserName,
                string.IsNullOrEmpty(review.Avatar)
                    ? await _imageStore.GetUrl("avatars/default.png", DateTime.UtcNow.AddMinutes(2))
                    : await _imageStore.GetUrl($"avatars/{review.Avatar}", DateTime.UtcNow.AddMinutes(2)),
                review.Timestamp > review.Created,
                review.Timestamp.ToString("O")
            ));
        }

        return new PaginatedList<ReviewVm>(
            result,
            count,
            request.PageNumber,
            request.PageSize
        );
    }
}
