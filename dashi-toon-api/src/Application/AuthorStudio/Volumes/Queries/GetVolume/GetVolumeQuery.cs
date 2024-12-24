using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.AuthorStudio.Volumes.Queries.GetVolume;

[Authorize]
public sealed record GetVolumeQuery(int SeriesId, int VolumeId) : IRequest<VolumeDetailVm>;

public sealed class GetVolumeQueryHandler : IRequestHandler<GetVolumeQuery, VolumeDetailVm>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;

    public GetVolumeQueryHandler(IApplicationDbContext dbContext, IUser user)
    {
        _dbContext = dbContext;
        _user = user;
    }

    public async Task<VolumeDetailVm> Handle(GetVolumeQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.Series? series = await _dbContext.Series
            .Include(s => s.Volumes.Where(v => v.Id == request.VolumeId))
            .FirstOrDefaultAsync(s => s.Id == request.SeriesId, cancellationToken);

        if (series == null)
        {
            throw new NotFoundException(request.SeriesId.ToString(), nameof(Series));
        }

        if (series.CreatedBy != _user.Id)
        {
            throw new ForbiddenAccessException();
        }

        if (!series.Volumes.Any())
        {
            throw new NotFoundException(request.VolumeId.ToString(), nameof(Volume));
        }

        Volume volume = series.Volumes[0];

        return new VolumeDetailVm(
            volume.Id,
            volume.VolumeNumber,
            volume.Name,
            volume.Introduction,
            volume.ChapterCount
        );
    }
}
