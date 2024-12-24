namespace DashiToon.Api.Application.AuthorStudio.Volumes.Queries.GetVolume;

public sealed record VolumeDetailVm(
    int VolumeId,
    int VolumeNumber,
    string Name,
    string? Introduction,
    int ChapterCount
);
