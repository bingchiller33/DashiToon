using DashiToon.Api.Application.AuthorStudio.Chapters.Models;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.ReadContent.Queries.GetComicChapter;

public sealed record ComicChapterDetailVm(
    int Id,
    int ChapterNumber,
    string Title,
    string? Thumbnail,
    int? Price,
    bool IsAdvanceChapter,
    List<ImageInfo> Content,
    ChapterStatus Status,
    string? PublishedDate
);
