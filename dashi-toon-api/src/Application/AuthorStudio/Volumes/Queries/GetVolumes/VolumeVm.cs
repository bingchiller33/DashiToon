namespace DashiToon.Api.Application.AuthorStudio.Volumes.Queries.GetVolumes;

public sealed record VolumeVm(
    int VolumeId,
    int VolumeNumber,
    string Name,
    string? Introduction,
    int ChapterCount
);
