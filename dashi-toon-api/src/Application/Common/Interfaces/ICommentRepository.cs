using DashiToon.Api.Application.Comments.Models;

namespace DashiToon.Api.Application.Common.Interfaces;

public interface ICommentRepository
{
    Task<(int Count, IEnumerable<CommentDto> Result)> FindComments(
        int chapterId,
        int pageNumber,
        int pageSize,
        string sortBy);

    Task<IEnumerable<ReplyDto>> FindCommentReplies(int chapterId, Guid commentId);
}
