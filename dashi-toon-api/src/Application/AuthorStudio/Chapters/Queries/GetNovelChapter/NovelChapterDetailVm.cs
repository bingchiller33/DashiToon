using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.Queries.GetNovelChapter;

public sealed record NovelChapterDetailVm(
    int Id,
    Guid VersionId,
    Guid? PublishedId,
    string Title,
    string? Thumbnail,
    string Content,
    string? Note,
    int? Price,
    ChapterStatus Status,
    string? PublishedDate,
    string Timestamp
);
