using DashiToon.Api.Application.Administrator.Missions.Models;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;

namespace DashiToon.Api.Application.Administrator.Missions.Queries.GetMissions;

[Authorize(Roles = Roles.Administrator)]
public sealed record GetMissionsQuery : IRequest<List<MissionVm>>;

public sealed class GetMissionsQueryHandler
    : IRequestHandler<GetMissionsQuery, List<MissionVm>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetMissionsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<MissionVm>> Handle(
        GetMissionsQuery request,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Missions
            .AsNoTracking()
            .Select(mission => new MissionVm(
                mission.Id,
                mission.ReadCount,
                mission.Reward,
                mission.IsActive
            )).ToListAsync(cancellationToken);
    }
}
