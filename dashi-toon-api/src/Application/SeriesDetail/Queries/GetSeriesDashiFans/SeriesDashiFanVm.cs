using DashiToon.Api.Domain.ValueObjects;

namespace DashiToon.Api.Application.SeriesDetail.Queries.GetSeriesDashiFans;

public sealed record SeriesDashiFanVm(
    Guid Id,
    string Name,
    Price Price,
    Price ActualPrice,
    string Description,
    int Perks
);
