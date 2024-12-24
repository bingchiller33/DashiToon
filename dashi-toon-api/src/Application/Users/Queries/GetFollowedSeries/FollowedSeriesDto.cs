using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.Users.Queries.GetFollowedSeries;

public class FollowedSeriesDto
{
    public string Thumbnail { get; init; } = null!;
    public string Title { get; init; } = null!;
    public SeriesType Type { get; set; }
    public SeriesStatus Status { get; init; }
    public int SeriesId { get; set; }
    public int? VolumeId { get; set; }
    public int? LatestChapterId { get; init; }
    public int Progress { get; init; }
    public int TotalChapters { get; init; }
    public bool IsNotified { get; init; }
}
