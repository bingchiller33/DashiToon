namespace DashiToon.Api.Application.SeriesDetail.Queries.GetSeriesChapters;

public sealed record SeriesChapterVm(
    int Id,
    int ChapterNumber,
    string Title,
    string? Thumbnail,
    string? PublishedDate,
    bool IsAdvanceChapter,
    int? Price
);
