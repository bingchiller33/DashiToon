namespace DashiToon.Api.Application.Comments.Models;

public class CommentDto
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = null!;
    public string UserName { get; init; } = null!;
    public string? Avatar { get; init; }
    public string Content { get; init; } = null!;
    public int Likes { get; init; }
    public int Dislikes { get; init; }
    public int RepliesCount { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public DateTimeOffset Created { get; init; }
    public DateTimeOffset LastModified { get; init; }
}
