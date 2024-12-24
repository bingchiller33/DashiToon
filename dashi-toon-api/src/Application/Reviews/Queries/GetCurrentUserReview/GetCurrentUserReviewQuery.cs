using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Reviews.Models;

namespace DashiToon.Api.Application.Reviews.Queries.GetCurrentUserReview;

[Authorize]
public sealed record GetCurrentUserReviewQuery(int SeriesId) : IRequest<ReviewVm?>;

public sealed class GetCurrentUserReviewQueryHandler : IRequestHandler<GetCurrentUserReviewQuery, ReviewVm?>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IImageStore _imageStore;
    private readonly IUser _user;

    public GetCurrentUserReviewQueryHandler(
        IReviewRepository reviewRepository,
        IImageStore imageStore,
        IUser user)
    {
        _reviewRepository = reviewRepository;
        _imageStore = imageStore;
        _user = user;
    }

    public async Task<ReviewVm?> Handle(GetCurrentUserReviewQuery request, CancellationToken cancellationToken)
    {
        ReviewDto? userReview = await _reviewRepository.FindReviewByUserAndSeries(request.SeriesId, _user.Id!);

        return userReview is null
            ? null
            : new ReviewVm(
                userReview.Id,
                userReview.Content,
                userReview.IsRecommended,
                userReview.Likes,
                userReview.Dislikes,
                userReview.UserId,
                userReview.UserName,
                string.IsNullOrEmpty(userReview.Avatar)
                    ? await _imageStore.GetUrl("avatars/default.png", DateTime.UtcNow.AddMinutes(2))
                    : await _imageStore.GetUrl($"avatars/{userReview.Avatar}", DateTime.UtcNow.AddMinutes(2)),
                userReview.Timestamp > userReview.Created,
                userReview.Timestamp.ToString("O")
            );
    }
}
