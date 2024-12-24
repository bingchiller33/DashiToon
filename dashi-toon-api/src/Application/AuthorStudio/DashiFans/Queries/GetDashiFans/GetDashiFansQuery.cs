using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;

namespace DashiToon.Api.Application.AuthorStudio.DashiFans.Queries.GetDashiFans;

[Authorize]
public sealed record GetDashiFansQuery(int SeriesId) : IRequest<List<DashiFanVm>>;

public sealed class GetDashiFansQueryHandler : IRequestHandler<GetDashiFansQuery, List<DashiFanVm>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;

    public GetDashiFansQueryHandler(IApplicationDbContext dbContext, IUser user)
    {
        _dbContext = dbContext;
        _user = user;
    }

    public async Task<List<DashiFanVm>> Handle(GetDashiFansQuery request, CancellationToken cancellationToken)
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

        return series.Tiers.Select(df => new DashiFanVm(
            df.Id,
            df.Name,
            df.Price,
            df.Description,
            df.Perks,
            df.IsActive,
            df.LastModified.ToString("O")
        )).ToList();
    }
}
