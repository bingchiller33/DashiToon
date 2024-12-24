using DashiToon.Api.Application.Comments.Models;
using DashiToon.Api.Application.Common.Interfaces;

namespace DashiToon.Api.Application.Comments.Queries.GetCommentReplies;

public sealed record GetCommentRepliesQuery(int ChapterId, Guid CommentId) : IRequest<List<ReplyVm>>;

public sealed class GetCommentRepliesQueryHandler : IRequestHandler<GetCommentRepliesQuery, List<ReplyVm>>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IImageStore _imageStore;

    public GetCommentRepliesQueryHandler(ICommentRepository commentRepository, IImageStore imageStore)
    {
        _commentRepository = commentRepository;
        _imageStore = imageStore;
    }

    public async Task<List<ReplyVm>> Handle(
        GetCommentRepliesQuery request,
        CancellationToken cancellationToken)
    {
        List<ReplyDto>? comments = (await _commentRepository.FindCommentReplies(request.ChapterId, request.CommentId))
            .ToList();

        Dictionary<Guid, string>? repliedUsernameMap = comments.ToDictionary(
            comment => comment.Id,
            comment => comment.UserName
        );

        List<ReplyVm>? result = new();

        ReplyDto? parentComment = comments.First(c => c.Depth == 1);

        foreach (ReplyDto? reply in BuildRepliesTree(parentComment.Id, comments)
                     .OrderByDescending(c => c.Likes - c.Dislikes))
        {
            result.Add(new ReplyVm(
                reply.Id,
                $"@{repliedUsernameMap.GetValueOrDefault(reply.ParentCommentId) ?? string.Empty} {reply.Content}",
                reply.Likes,
                reply.Dislikes,
                reply.UserId,
                reply.UserName,
                string.IsNullOrEmpty(reply.Avatar)
                    ? await _imageStore.GetUrl("avatars/default.png", DateTime.UtcNow.AddMinutes(2))
                    : await _imageStore.GetUrl($"avatars/{reply.Avatar}", DateTime.UtcNow.AddMinutes(2)),
                reply.Timestamp > reply.Created,
                reply.Timestamp.ToString("O")
            ));
        }

        return result
            .OrderByDescending(c => c.Likes - c.Dislikes)
            .ToList();
    }

    private static IEnumerable<ReplyDto> BuildRepliesTree(
        Guid parentId,
        List<ReplyDto> comments)
    {
        List<ReplyDto>? childComments = comments
            .Where(c => c.ParentCommentId == parentId)
            .ToList();

        foreach (ReplyDto? childComment in childComments)
        {
            yield return childComment;

            foreach (ReplyDto? reply in BuildRepliesTree(childComment.Id, comments))
            {
                yield return reply;
            }
        }
    }
}
