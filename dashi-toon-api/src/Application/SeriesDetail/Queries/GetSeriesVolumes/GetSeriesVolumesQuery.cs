using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.SeriesDetail.Queries.GetSeriesVolumes;

public sealed record GetSeriesVolumesQuery(int SeriesId) : IRequest<List<SeriesVolumeVm>>;

public sealed class GetSeriesVolumesQueryHandler : IRequestHandler<GetSeriesVolumesQuery, List<SeriesVolumeVm>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSeriesVolumesQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<SeriesVolumeVm>> Handle(
        GetSeriesVolumesQuery request,
        CancellationToken cancellationToken)
    {
        Series? series = await _dbContext.Series
            .Include(v => v.Volumes)
            .FirstOrDefaultAsync(
                s => s.Id == request.SeriesId && s.Status != SeriesStatus.Trashed && s.Status != SeriesStatus.Hiatus &&
                     s.Status != SeriesStatus.Draft,
                cancellationToken);

        if (series is null)
        {
            return [];
        }

        List<SeriesVolumeVm> volumes = await _dbContext.Volumes
            .Where(v => v.SeriesId == request.SeriesId)
            .OrderByDescending(v => v.VolumeNumber)
            .Select(v => new SeriesVolumeVm(
                v.Id,
                v.VolumeNumber,
                v.Name,
                v.Introduction
            ))
            .ToListAsync(cancellationToken);

        return volumes;
    }
}
