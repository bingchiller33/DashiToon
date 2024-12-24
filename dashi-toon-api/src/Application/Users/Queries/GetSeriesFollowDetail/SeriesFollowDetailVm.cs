namespace DashiToon.Api.Application.Users.Queries.GetSeriesFollowDetail;

public sealed record SeriesFollowDetailVm(
    bool IsFollowed,
    FollowDetailVm? Detail
);

public sealed record FollowDetailVm(
    int SeriesId,
    int? LatestVolumeReadId,
    int? LatestChapterReadId,
    int Progress,
    int TotalChapters,
    bool IsNotified);

public sealed class FollowDetailDto
{
    public int SeriesId { get; init; }
    public int? VolumeId { get; init; }
    public int? LatestChapterId { get; init; }
    public int Progress { get; init; }
    public int TotalChapters { get; init; }
    public bool IsNotified { get; init; }
}
