using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;

[Authorize]
[Restrict(Require = Restrictions.NotRestricted)]
public sealed record CreateVolumeCommand(int SeriesId, string Name, string Introduction) : IRequest<int>;

public sealed class CreateVolumeCommandHandler : IRequestHandler<CreateVolumeCommand, int>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;

    public CreateVolumeCommandHandler(IApplicationDbContext dbContext, IUser user)
    {
        _dbContext = dbContext;
        _user = user;
    }

    public async Task<int> Handle(CreateVolumeCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Series? series = await _dbContext.Series
            .Include(x => x.Volumes)
            .FirstOrDefaultAsync(x => x.Id == request.SeriesId, cancellationToken);

        if (series is null)
        {
            throw new NotFoundException(request.SeriesId.ToString(), nameof(Series));
        }

        if (series.CreatedBy != _user.Id)
        {
            throw new ForbiddenAccessException();
        }

        Volume volume = Volume.Create(request.Name, request.Introduction);

        series.AddNewVolume(volume);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return volume.Id;
    }
}
