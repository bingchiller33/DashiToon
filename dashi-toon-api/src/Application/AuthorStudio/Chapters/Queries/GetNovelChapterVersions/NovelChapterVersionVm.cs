namespace DashiToon.Api.Application.AuthorStudio.Chapters.Queries.GetNovelChapterVersions;

public sealed record NovelChapterVersionVm(
    Guid VersionId,
    string VersionName,
    bool IsCurrent,
    bool IsPublished,
    bool IsAutoSave,
    string Title,
    string? Thumbnail,
    string Content,
    string? Note,
    string Timestamp);
