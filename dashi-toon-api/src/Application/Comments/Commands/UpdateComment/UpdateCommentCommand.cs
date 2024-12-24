using DashiToon.Api.Application.Comments.Models;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Comments.Commands.UpdateComment;

[Authorize]
[Restrict(Require = Restrictions.NotMuted)]
public sealed record UpdateCommentCommand(int ChapterId, Guid CommentId, string Content) : IRequest<CommentVm>;

public sealed class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, CommentVm>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserRepository _userRepository;
    private readonly IImageStore _imageStore;
    private readonly IUser _user;

    public UpdateCommentCommandHandler(
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

    public async Task<CommentVm> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        Comment? comment = await _dbContext.Comments
            .FirstOrDefaultAsync(c => c.ChapterId == request.ChapterId && c.Id == request.CommentId, cancellationToken);

        if (comment is null)
        {
            throw new NotFoundException(request.CommentId.ToString(), nameof(Comment));
        }

        if (comment.CreatedBy != _user.Id)
        {
            throw new ForbiddenAccessException();
        }

        comment.Update(request.Content);

        await _dbContext.SaveChangesAsync(cancellationToken);

        string? avatar = await _userRepository.GetUserAvatarById(comment.UserId);

        return new CommentVm(
            comment.Id,
            comment.Content,
            comment.Likes,
            comment.Dislikes,
            comment.Replies.Count,
            _user.Id!,
            _user.Name!,
            string.IsNullOrEmpty(avatar)
                ? await _imageStore.GetUrl("avatars/default.png", DateTime.UtcNow.AddMinutes(2))
                : await _imageStore.GetUrl($"avatars/{avatar}", DateTime.UtcNow.AddMinutes(2)),
            comment.Timestamp > comment.Created,
            comment.Timestamp.ToString("O")
        );
    }
}
