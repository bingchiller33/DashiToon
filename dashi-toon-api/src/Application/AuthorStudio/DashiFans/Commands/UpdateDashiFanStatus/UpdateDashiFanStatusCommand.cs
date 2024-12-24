using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Exceptions;

namespace DashiToon.Api.Application.AuthorStudio.DashiFans.Commands.UpdateDashiFanStatus;

[Authorize]
public sealed record UpdateDashiFanStatusCommand(
    int SeriesId,
    Guid TierId)
    : IRequest;

public sealed class UpdateDashiFanStatusCommandHandler : IRequestHandler<UpdateDashiFanStatusCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;

    public UpdateDashiFanStatusCommandHandler(IApplicationDbContext dbContext, IUser user)
    {
        _dbContext = dbContext;
        _user = user;
    }

    public async Task Handle(UpdateDashiFanStatusCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Series? series = await _dbContext.Series
            .Include(s => s.Tiers.Where(t => t.Id == request.TierId))
            .FirstOrDefaultAsync(s => s.Id == request.SeriesId, cancellationToken);

        if (series is null)
        {
            throw new NotFoundException(request.SeriesId.ToString(), nameof(Series));
        }

        if (series.CreatedBy != _user.Id)
        {
            throw new ForbiddenAccessException();
        }

        if (!series.Tiers.Any())
        {
            throw new DashiFanTierNotFoundException();
        }

        series.Tiers[0].ChangeStatus();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
