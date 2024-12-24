using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Reviews.Models;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Reviews.Commands.UpdateSeriesReviewCommand;

[Authorize]
[Restrict(Require = Restrictions.NotMuted)]
public sealed record UpdateReviewCommand(
    int SeriesId,
    Guid ReviewId,
    string Content,
    bool IsRecommended) : IRequest<ReviewVm>;

public sealed class UpdateReviewCommandHandler : IRequestHandler<UpdateReviewCommand, ReviewVm>
{
    private readonly ISeriesRepository _seriesRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserRepository _userRepository;
    private readonly ISearchService _searchService;
    private readonly IImageStore _imageStore;
    private readonly IUser _user;

    public UpdateReviewCommandHandler(
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

    public async Task<ReviewVm> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
    {
        Series? series = await _dbContext.Series.FirstOrDefaultAsync(
            s => s.Id == request.SeriesId,
            cancellationToken);

        if (series is null)
        {
            throw new NotFoundException(request.SeriesId.ToString(), nameof(Series));
        }

        Review? review = await _dbContext.Reviews.FirstOrDefaultAsync(
            r => r.Id == request.ReviewId && r.SeriesId == request.SeriesId,
            cancellationToken);

        if (review is null)
        {
            throw new NotFoundException(request.ReviewId.ToString(), nameof(Review));
        }

        if (review.CreatedBy != _user.Id)
        {
            throw new ForbiddenAccessException();
        }

        review.Update(request.Content, request.IsRecommended);

        _dbContext.Reviews.Update(review);

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
