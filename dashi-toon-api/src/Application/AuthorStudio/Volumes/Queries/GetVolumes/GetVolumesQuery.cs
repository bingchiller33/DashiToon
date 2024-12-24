using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;

namespace DashiToon.Api.Application.AuthorStudio.Volumes.Queries.GetVolumes;

[Authorize]
public sealed record GetVolumesQuery(int SeriesId) : IRequest<List<VolumeVm>>;

public sealed class GetVolumesQueryHandler : IRequestHandler<GetVolumesQuery, List<VolumeVm>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;

    public GetVolumesQueryHandler(IApplicationDbContext dbContext, IUser user)
    {
        _dbContext = dbContext;
        _user = user;
    }

    public async Task<List<VolumeVm>> Handle(GetVolumesQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.Series? series = await _dbContext.Series
            .Include(s => s.Volumes)
            .FirstOrDefaultAsync(s => s.Id == request.SeriesId, cancellationToken);

        if (series is null)
        {
            throw new NotFoundException(request.SeriesId.ToString(), nameof(Series));
        }

        if (series.CreatedBy != _user.Id)
        {
            throw new ForbiddenAccessException();
        }

        return series.Volumes
            .OrderBy(v => v.VolumeNumber)
            .Select(v => new VolumeVm(
                v.Id,
                v.VolumeNumber,
                v.Name,
                v.Introduction,
                v.ChapterCount))
            .ToList();
    }
}
