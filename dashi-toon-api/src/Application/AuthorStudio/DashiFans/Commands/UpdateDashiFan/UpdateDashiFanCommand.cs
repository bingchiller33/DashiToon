using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Exceptions;

namespace DashiToon.Api.Application.AuthorStudio.DashiFans.Commands.UpdateDashiFan;

[Authorize]
public sealed record UpdateDashiFanCommand(
    int SeriesId,
    Guid TierId,
    string Name,
    string Description,
    int Perks,
    decimal Amount)
    : IRequest;

public sealed class UpdateDashiFanCommandHandler : IRequestHandler<UpdateDashiFanCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;

    public UpdateDashiFanCommandHandler(IApplicationDbContext dbContext, IUser user)
    {
        _dbContext = dbContext;
        _user = user;
    }

    public async Task Handle(UpdateDashiFanCommand request, CancellationToken cancellationToken)
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

        series.Tiers[0].Update(
            request.Name,
            request.Description,
            request.Perks,
            request.Amount);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
