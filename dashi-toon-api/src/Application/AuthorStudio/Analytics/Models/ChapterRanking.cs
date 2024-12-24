namespace DashiToon.Api.Application.AuthorStudio.Analytics.Models;

public sealed record ChapterRankings(
    int Id,
    int ChapterNumber,
    string Name,
    int VolumeId,
    string VolumeName,
    int Data,
    int? Compare,
    int Rank
);

public sealed class ChapterRankingsDto
{
    public int Id { get; init; }
    public int ChapterNumber { get; init; }
    public string Name { get; init; } = null!;
    public int VolumeId { get; init; }
    public string VolumeName { get; init; } = null!;
    public int Data { get; init; }
    public int Rank { get; init; }
}
