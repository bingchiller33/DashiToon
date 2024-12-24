using DashiToon.Api.Application.AuthorStudio.Chapters.Models;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.Queries.GetComicChapter;

public sealed record ComicChapterDetailVm(
    int Id,
    Guid VersionId,
    Guid? PublishedId,
    string Title,
    string? Thumbnail,
    List<ImageInfo> Content,
    string? Note,
    int? Price,
    ChapterStatus Status,
    string? PublishedDate,
    string Timestamp
);
