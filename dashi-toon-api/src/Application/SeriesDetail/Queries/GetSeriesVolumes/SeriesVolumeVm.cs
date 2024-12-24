namespace DashiToon.Api.Application.SeriesDetail.Queries.GetSeriesVolumes;

public sealed record SeriesVolumeVm(
    int VolumeId,
    int VolumeNumber,
    string Name,
    string? Introduction
);
