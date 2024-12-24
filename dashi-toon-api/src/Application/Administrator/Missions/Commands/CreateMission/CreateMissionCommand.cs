using DashiToon.Api.Application.Administrator.Missions.Models;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DashiToon.Api.Application.Administrator.Missions.Commands.CreateMission;

[Authorize(Roles = Roles.Administrator)]
public sealed record CreateMissionCommand(
    int ReadCount,
    int Reward,
    bool IsActive = true
) : IRequest<MissionVm>;

public sealed class CreateMissionCommandHandler
    : IRequestHandler<CreateMissionCommand, MissionVm>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateMissionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MissionVm> Handle(
        CreateMissionCommand request,
        CancellationToken cancellationToken)
    {
        Mission? existingMission = await _dbContext.Missions
            .FirstOrDefaultAsync(x => x.ReadCount == request.ReadCount,
                cancellationToken);

        if (existingMission != null)
        {
            throw new ArgumentException(
                $"A mission with ReadCount {request.ReadCount} already exists"
            );
        }

        Mission? mission = Mission.CreateNew(
            request.ReadCount,
            request.Reward,
            request.IsActive
        );

        _dbContext.Missions.Add(mission);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new MissionVm(
            mission.Id,
            mission.ReadCount,
            mission.Reward,
            mission.IsActive
        );
    }
}
