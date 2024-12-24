using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Reviews.Models;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Reviews.Commands.RateReviewCommand;

[Authorize]
[Restrict(Require = Restrictions.NotMuted)]
public sealed record RateReviewCommand(int SeriesId, Guid ReviewId, bool IsLiked) : IRequest<ReviewVm>;

public sealed class RateReviewCommandHandler : IRequestHandler<RateReviewCommand, ReviewVm>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserRepository _userRepository;
    private readonly IImageStore _imageStore;
    private readonly IUser _user;

    public RateReviewCommandHandler(
        IApplicationDbContext dbContext,
        IUserRepository userRepository,
        IImageStore imageStore,
        IUser user)
    {
        _dbContext = dbContext;
        _userRepository = userRepository;
        _imageStore = imageStore;
        _user = user;
    }

    public async Task<ReviewVm> Handle(RateReviewCommand request, CancellationToken cancellationToken)
    {
        Series? series = await _dbContext.Series.FirstOrDefaultAsync(s => s.Id == request.SeriesId,
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

        review.Rate(_user.Id!, request.IsLiked);
        await _dbContext.SaveChangesAsync(cancellationToken);

        string? avatar = await _userRepository.GetUserAvatarById(review.UserId);

        return new ReviewVm(
            review.Id,
            review.Content,
            review.IsRecommended,
            review.Likes,
            review.Dislikes,
            review.UserId,
            await _userRepository.GetUsernameById(review.UserId) ?? string.Empty,
            string.IsNullOrEmpty(avatar)
                ? await _imageStore.GetUrl("avatars/default.png", DateTime.UtcNow.AddMinutes(2))
                : await _imageStore.GetUrl($"avatars/{avatar}", DateTime.UtcNow.AddMinutes(2)),
            review.Timestamp > review.Created,
            review.Timestamp.ToString("O")
        );
    }
}
