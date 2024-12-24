using DashiToon.Api.Application.Administrator.Missions.Models;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DashiToon.Api.Application.Administrator.Missions.Commands.UpdateMission;

[Authorize(Roles = Roles.Administrator)]
public sealed record UpdateMissionCommand(
    Guid Id,
    int ReadCount,
    int Reward,
    bool IsActive
) : IRequest<MissionVm>;

public sealed class UpdateMissionCommandHandler
    : IRequestHandler<UpdateMissionCommand, MissionVm>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateMissionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MissionVm> Handle(
        UpdateMissionCommand request,
        CancellationToken cancellationToken)
    {
        Mission? mission = await _dbContext.Missions
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (mission is null)
        {
            throw new NotFoundException(request.Id.ToString(), nameof(Mission));
        }

        // Check if another mission with same ReadCount exists
        Mission? existingMission = await _dbContext.Missions
            .FirstOrDefaultAsync(x =>
                    x.Id != request.Id &&
                    x.ReadCount == request.ReadCount,
                cancellationToken);

        if (existingMission != null)
        {
            throw new ArgumentException(
                $"Another mission with ReadCount {request.ReadCount} already exists"
            );
        }

        mission.Update(
            request.ReadCount,
            request.Reward,
            request.IsActive
        );

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new MissionVm(
            mission.Id,
            mission.ReadCount,
            mission.Reward,
            mission.IsActive
        );
    }
}
