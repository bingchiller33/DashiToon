using DashiToon.Api.Application.AuthorStudio.Chapters.Models;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.Queries.PreviewComicChapter;

public sealed record ComicChapterPreviewVm(
    int ChapterId,
    Guid VersionId,
    string VersionName,
    string Title,
    string? Thumbnail,
    List<ImageInfo> Content,
    string? Note
);
