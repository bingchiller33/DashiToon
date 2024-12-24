using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.AuthorStudio.DashiFans.Commands.CreateDashiFan;

[Authorize]
public sealed record CreateDashiFanCommand(
    int SeriesId,
    string Name,
    string Description,
    int Perks,
    decimal Amount,
    string Currency
) : IRequest<Guid>;

public sealed class CreateDashiFanCommandHandler : IRequestHandler<CreateDashiFanCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;

    public CreateDashiFanCommandHandler(IApplicationDbContext dbContext, IUser user)
    {
        _dbContext = dbContext;
        _user = user;
    }

    public async Task<Guid> Handle(CreateDashiFanCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Series? series = await _dbContext.Series
            .Include(s => s.Tiers)
            .FirstOrDefaultAsync(s => s.Id == request.SeriesId, cancellationToken);

        if (series is null)
        {
            throw new NotFoundException(request.SeriesId.ToString(), nameof(Series));
        }

        if (series.CreatedBy != _user.Id)
        {
            throw new ForbiddenAccessException();
        }

        DashiFan tier = DashiFan.Create(
            request.Name,
            request.Description,
            request.Perks,
            request.Amount,
            request.Currency);

        series.AddNewDashiFan(tier);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return tier.Id;
    }
}
