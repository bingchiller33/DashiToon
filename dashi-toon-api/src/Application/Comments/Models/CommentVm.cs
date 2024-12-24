namespace DashiToon.Api.Application.Comments.Models;

public sealed record CommentVm(
    Guid Id,
    string Content,
    int Likes,
    int Dislikes,
    int RepliesCount,
    string UserId,
    string Username,
    string UserAvatar,
    bool IsEdited,
    string CommentDate
);
