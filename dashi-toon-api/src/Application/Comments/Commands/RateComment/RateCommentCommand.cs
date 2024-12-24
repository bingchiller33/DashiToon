using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Comments.Models;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Comments.Commands.RateComment;

[Authorize]
[Restrict(Require = Restrictions.NotMuted)]
public sealed record RateCommentCommand(int ChapterId, Guid CommentId, bool IsLiked) : IRequest<CommentVm>;

public sealed class RateCommentCommandHandler : IRequestHandler<RateCommentCommand, CommentVm>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserRepository _userRepository;
    private readonly IImageStore _imageStore;
    private readonly IUser _user;

    public RateCommentCommandHandler(
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

    public async Task<CommentVm> Handle(RateCommentCommand request, CancellationToken cancellationToken)
    {
        Comment? comment = await _dbContext.Comments
            .FirstOrDefaultAsync(c => c.ChapterId == request.ChapterId && c.Id == request.CommentId, cancellationToken);

        if (comment is null)
        {
            throw new NotFoundException(request.CommentId.ToString(), nameof(Comment));
        }

        comment.Rate(_user.Id!, request.IsLiked);
        await _dbContext.SaveChangesAsync(cancellationToken);

        string? avatar = await _userRepository.GetUserAvatarById(comment.UserId);

        return new CommentVm(
            comment.Id,
            comment.Content,
            comment.Likes,
            comment.Dislikes,
            comment.Replies.Count,
            comment.UserId,
            await _userRepository.GetUsernameById(comment.UserId) ?? string.Empty,
            string.IsNullOrEmpty(avatar)
                ? await _imageStore.GetUrl("avatars/default.png", DateTime.UtcNow.AddMinutes(2))
                : await _imageStore.GetUrl($"avatars/{avatar}", DateTime.UtcNow.AddMinutes(2)),
            comment.Timestamp > comment.Created,
            comment.Timestamp.ToString("O")
        );
    }
}
