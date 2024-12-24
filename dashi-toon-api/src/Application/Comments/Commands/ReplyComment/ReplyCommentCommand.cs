using DashiToon.Api.Application.Comments.Models;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Comments.Commands.ReplyComment;

[Authorize]
[Restrict(Require = Restrictions.NotMuted)]
public sealed record ReplyCommentCommand(int ChapterId, Guid CommentId, string Content) : IRequest<CommentVm>;

public sealed class ReplyCommentCommandHandler : IRequestHandler<ReplyCommentCommand, CommentVm>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserRepository _userRepository;
    private readonly IImageStore _imageStore;
    private readonly IUser _user;

    public ReplyCommentCommandHandler(
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

    public async Task<CommentVm> Handle(ReplyCommentCommand request, CancellationToken cancellationToken)
    {
        Comment? comment = await _dbContext.Comments
            .FirstOrDefaultAsync(c => c.ChapterId == request.ChapterId && c.Id == request.CommentId, cancellationToken);

        if (comment is null)
        {
            throw new NotFoundException(request.CommentId.ToString(), nameof(Comment));
        }

        comment.ReplyComment(_user.Id!, request.Content);

        await _dbContext.SaveChangesAsync(cancellationToken);

        Comment? reply = comment.Replies[^1];

        string? avatar = await _userRepository.GetUserAvatarById(reply.UserId);

        return new CommentVm(
            reply.Id,
            reply.Content,
            reply.Likes,
            reply.Dislikes,
            reply.Replies.Count,
            reply.UserId,
            await _userRepository.GetUsernameById(reply.UserId) ?? string.Empty,
            string.IsNullOrEmpty(avatar)
                ? await _imageStore.GetUrl("avatars/default.png", DateTime.UtcNow.AddMinutes(2))
                : await _imageStore.GetUrl($"avatars/{avatar}", DateTime.UtcNow.AddMinutes(2)),
            reply.Timestamp > reply.Created,
            reply.Timestamp.ToString("O")
        );
    }
}
