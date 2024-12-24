using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.Users.Queries.GetFollowedSeries;

public sealed record FollowedSeriesVm(
    string Thumbnail,
    string Title,
    SeriesType Type,
    SeriesStatus Status,
    int SeriesId,
    int? LatestVolumeReadId,
    int? LatestChapterReadId,
    int Progress,
    int TotalChapters,
    bool IsNotified
);
