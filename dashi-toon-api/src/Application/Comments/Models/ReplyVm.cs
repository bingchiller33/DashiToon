namespace DashiToon.Api.Application.Comments.Models;

public sealed record ReplyVm(
    Guid Id,
    string Content,
    int Likes,
    int Dislikes,
    string UserId,
    string Username,
    string UserAvatar,
    bool IsEdited,
    string CommentDate);
