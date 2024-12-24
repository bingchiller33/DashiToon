using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.Models;

public class ChapterInfo
{
    public int ChapterId { get; init; }
    public int ChapterNumber { get; init; }
    public required string Title { get; init; }
    public string? Thumbnail { get; init; }
    public int? KanaPrice { get; init; }
    public ChapterStatus Status { get; init; }
    public DateTimeOffset? PublishedDate { get; init; }
}
