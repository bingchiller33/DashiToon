using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.AuthorStudio.Volumes.Commands.DeleteVolume;

[Authorize]
[Restrict(Require = Restrictions.NotRestricted)]
public sealed record DeleteVolumeCommand(int SeriesId, int VolumeId) : IRequest;

public sealed class DeleteVolumeCommandHandler : IRequestHandler<DeleteVolumeCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;

    public DeleteVolumeCommandHandler(IApplicationDbContext dbContext, IUser user)
    {
        _dbContext = dbContext;
        _user = user;
    }

    public async Task Handle(DeleteVolumeCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Series? series = await _dbContext.Series
            .Include(s => s.Volumes)
            .FirstOrDefaultAsync(s => s.Id == request.SeriesId, cancellationToken);

        if (series is null)
        {
            throw new NotFoundException(request.SeriesId.ToString(), nameof(Domain.Entities.Series));
        }

        if (series.CreatedBy != _user.Id)
        {
            throw new ForbiddenAccessException();
        }

        if (series.Volumes.All(v => v.Id != request.VolumeId))
        {
            throw new NotFoundException(request.VolumeId.ToString(), nameof(Volume));
        }

        series.RemoveVolume(request.VolumeId);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
