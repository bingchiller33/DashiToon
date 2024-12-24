using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.Queries.GetChapters;

public sealed record ChapterVm(
    int Id,
    int ChapterNumber,
    string Title,
    string Thumbnail,
    int? Price,
    ChapterStatus Status,
    string? PublishedDate
);
