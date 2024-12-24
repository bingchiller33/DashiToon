using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.ReadContent.Queries.Models;

public class ChapterContent
{
    public int ChapterId { get; init; }
    public int ChapterNumber { get; init; }
    public required string Title { get; init; }
    public string? Thumbnail { get; init; }
    public required string Content { get; init; }
    public int? KanaPrice { get; init; }
    public ChapterStatus Status { get; init; }
    public DateTimeOffset? PublishedDate { get; init; }
}
