using DashiToon.Api.Application.Comments.Models;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Comments.Commands.WriteComment;

[Authorize]
[Restrict(Require = Restrictions.NotMuted)]
public sealed record WriteCommentCommand(int ChapterId, string Content)
    : IRequest<CommentVm>;

public sealed class WriteCommentCommandHandler : IRequestHandler<WriteCommentCommand, CommentVm>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserRepository _userRepository;
    private readonly IImageStore _imageStore;
    private readonly IUser _user;

    public WriteCommentCommandHandler(
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

    public async Task<CommentVm> Handle(WriteCommentCommand request, CancellationToken cancellationToken)
    {
        Chapter? chapter =
            await _dbContext.Chapters.FirstOrDefaultAsync(c => c.Id == request.ChapterId, cancellationToken);

        if (chapter is null)
        {
            throw new NotFoundException(request.ChapterId.ToString(), nameof(Chapter));
        }

        Comment? comment = Comment.Create(
            _user.Id!,
            chapter.Id,
            request.Content
        );

        _dbContext.Comments.Add(comment);
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
