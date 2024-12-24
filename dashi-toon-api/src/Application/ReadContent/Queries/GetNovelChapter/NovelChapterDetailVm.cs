using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.ReadContent.Queries.GetNovelChapter;

public sealed record NovelChapterDetailVm(
    int Id,
    int ChapterNumber,
    string Title,
    string? Thumbnail,
    int? Price,
    bool IsAdvanceChapter,
    string Content,
    ChapterStatus Status,
    string? PublishedDate
);
