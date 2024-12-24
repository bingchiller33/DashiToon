using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Reviews.Models;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Exceptions;

namespace DashiToon.Api.Application.Reviews.Commands.WriteSeriesReviews;

[Authorize]
[Restrict(Require = Restrictions.NotMuted)]
public sealed record WriteSeriesReviewCommand(int SeriesId, string Content, bool IsRecommended)
    : IRequest<ReviewVm>;

public sealed class WriteSeriesReviewCommandHandler : IRequestHandler<WriteSeriesReviewCommand, ReviewVm>
{
    private readonly ISeriesRepository _seriesRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserRepository _userRepository;
    private readonly ISearchService _searchService;
    private readonly IImageStore _imageStore;
    private readonly IUser _user;

    public WriteSeriesReviewCommandHandler(
        ISeriesRepository seriesRepository,
        IApplicationDbContext dbContext,
        IUserRepository userRepository,
        ISearchService searchService,
        IImageStore imageStore,
        IUser user)
    {
        _seriesRepository = seriesRepository;
        _dbContext = dbContext;
        _userRepository = userRepository;
        _searchService = searchService;
        _imageStore = imageStore;
        _user = user;
    }


    public async Task<ReviewVm> Handle(WriteSeriesReviewCommand request, CancellationToken cancellationToken)
    {
        Series? series = await _dbContext.Series.FirstOrDefaultAsync(s => s.Id == request.SeriesId, cancellationToken);

        if (series is null)
        {
            throw new NotFoundException(request.SeriesId.ToString(), nameof(Series));
        }

        Review? existingReview = await _dbContext.Reviews.FirstOrDefaultAsync(
            r => r.UserId == _user.Id
                 && r.SeriesId == request.SeriesId, cancellationToken);

        if (existingReview != null)
        {
            throw new AlreadyReviewedException();
        }

        Review? review = Review.Create(
            _user.Id!,
            isRecommended: request.IsRecommended,
            content: request.Content,
            seriesId: request.SeriesId
        );

        _dbContext.Reviews.Add(review);
        await _dbContext.SaveChangesAsync(cancellationToken);

        double rating = await _seriesRepository.GetSeriesRating(review.SeriesId);

        await _searchService.UpdateSeriesRatingAsync(review.SeriesId, rating);

        string? avatar = await _userRepository.GetUserAvatarById(_user.Id!);

        return new ReviewVm(
            review.Id,
            review.Content,
            review.IsRecommended,
            review.Likes,
            review.Dislikes,
            _user.Id!,
            _user.Name!,
            string.IsNullOrEmpty(avatar)
                ? await _imageStore.GetUrl("avatars/default.png", DateTime.UtcNow.AddMinutes(2))
                : await _imageStore.GetUrl($"avatars/{avatar}", DateTime.UtcNow.AddMinutes(2)),
            review.Timestamp > review.Created,
            review.Timestamp.ToString("O")
        );
    }
}
