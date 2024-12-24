using DashiToon.Api.Application.Administrator.Missions.Models;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DashiToon.Api.Application.Administrator.Missions.Commands.UpdateMissionStatus;

[Authorize(Roles = Roles.Administrator)]
public sealed record UpdateMissionStatusCommand(
    Guid Id,
    bool IsActive
) : IRequest<MissionVm>;

public sealed class UpdateMissionStatusCommandHandler
    : IRequestHandler<UpdateMissionStatusCommand, MissionVm>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateMissionStatusCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MissionVm> Handle(
        UpdateMissionStatusCommand request,
        CancellationToken cancellationToken)
    {
        Mission? mission = await _dbContext.Missions
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (mission is null)
        {
            throw new NotFoundException(request.Id.ToString(), nameof(Mission));
        }

        mission.UpdateStatus(request.IsActive);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new MissionVm(
            mission.Id,
            mission.ReadCount,
            mission.Reward,
            mission.IsActive
        );
    }
}
