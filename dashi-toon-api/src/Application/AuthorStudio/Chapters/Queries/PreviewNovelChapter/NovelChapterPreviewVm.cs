namespace DashiToon.Api.Application.AuthorStudio.Chapters.Queries.PreviewNovelChapter;

public sealed record NovelChapterPreviewVm(
    int ChapterId,
    Guid VersionId,
    string VersionName,
    string Title,
    string? Thumbnail,
    string Content,
    string? Note
);
