using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.AuthorStudio.Volumes.Commands.UpdateVolume;

[Authorize]
[Restrict(Require = Restrictions.NotRestricted)]
public sealed record UpdateVolumeCommand(int SeriesId, int VolumeId, string Name, string? Introduction) : IRequest;

public sealed class UpdateVolumeCommandHandler : IRequestHandler<UpdateVolumeCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;

    public UpdateVolumeCommandHandler(IApplicationDbContext dbContext, IUser user)
    {
        _dbContext = dbContext;
        _user = user;
    }

    public async Task Handle(UpdateVolumeCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Series? series = await _dbContext.Series
            .Include(s => s.Volumes.Where(v => v.Id == request.VolumeId))
            .FirstOrDefaultAsync(x => x.Id == request.SeriesId, cancellationToken);

        if (series is null)
        {
            throw new NotFoundException(request.SeriesId.ToString(), nameof(Domain.Entities.Series));
        }

        if (series.CreatedBy != _user.Id)
        {
            throw new ForbiddenAccessException();
        }

        if (!series.Volumes.Any())
        {
            throw new NotFoundException(request.VolumeId.ToString(), nameof(Volume));
        }

        series.Volumes[0].Update(request.Name, request.Introduction);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
