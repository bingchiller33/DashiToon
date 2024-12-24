namespace DashiToon.Api.Application.Reviews.Models;

public sealed record ReviewVm(
    Guid Id,
    string Content,
    bool IsRecommended,
    int Likes,
    int Dislikes,
    string UserId,
    string Username,
    string UserAvatar,
    bool IsEdited,
    string ReviewDate
);
