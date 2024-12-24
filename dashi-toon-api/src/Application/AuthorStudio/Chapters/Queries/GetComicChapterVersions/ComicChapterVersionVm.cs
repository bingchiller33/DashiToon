using DashiToon.Api.Application.AuthorStudio.Chapters.Models;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.Queries.GetComicChapterVersions;

public sealed record ComicChapterVersionVm(
    Guid VersionId,
    string VersionName,
    bool IsCurrent,
    bool IsPublished,
    bool IsAutoSave,
    string Title,
    string? Thumbnail,
    List<ImageInfo> Content,
    string? Note,
    string Timestamp
);
