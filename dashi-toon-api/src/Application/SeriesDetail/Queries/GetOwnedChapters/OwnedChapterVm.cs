namespace DashiToon.Api.Application.SeriesDetail.Queries.GetOwnedChapters;

public sealed record OwnedChapterVm(
    int Id,
    int ChapterNumber,
    string Title,
    string? Thumbnail,
    string? PublishedDate,
    int? Price,
    bool Owned
);
