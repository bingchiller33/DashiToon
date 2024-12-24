using DashiToon.Api.Application.Comments.Models;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Models;

namespace DashiToon.Api.Application.Comments.Queries.GetComments;

public sealed record GetChapterCommentsQuery(
    int ChapterId,
    int PageNumber = 1,
    int PageSize = 10,
    string SortBy = "Relevance") : IRequest<PaginatedList<CommentVm>>;

public sealed class GetChapterCommentsQueryHandler : IRequestHandler<GetChapterCommentsQuery, PaginatedList<CommentVm>>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IImageStore _imageStore;

    public GetChapterCommentsQueryHandler(ICommentRepository commentRepository, IImageStore imageStore)
    {
        _commentRepository = commentRepository;
        _imageStore = imageStore;
    }

    public async Task<PaginatedList<CommentVm>> Handle(
        GetChapterCommentsQuery request,
        CancellationToken cancellationToken)
    {
        (int count, IEnumerable<CommentDto> comments) = await _commentRepository.FindComments(
            request.ChapterId,
            request.PageNumber,
            request.PageSize,
            request.SortBy);

        List<CommentVm>? result = new(request.PageSize);

        foreach (CommentDto? comment in comments)
        {
            result.Add(new CommentVm(
                comment.Id,
                comment.Content,
                comment.Likes,
                comment.Dislikes,
                comment.RepliesCount,
                comment.UserId,
                comment.UserName,
                string.IsNullOrEmpty(comment.Avatar)
                    ? await _imageStore.GetUrl("avatars/default.png", DateTime.UtcNow.AddMinutes(2))
                    : await _imageStore.GetUrl($"avatars/{comment.Avatar}", DateTime.UtcNow.AddMinutes(2)),
                comment.Timestamp > comment.Created,
                comment.Timestamp.ToString("O")
            ));
        }

        return new PaginatedList<CommentVm>(
            result,
            count,
            request.PageNumber,
            request.PageSize
        );
    }
}
