using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Services;

namespace DashiToon.Api.Application.Users.Commands.CompleteMission;

[Authorize]
public record CompleteMissionCommand(Guid MissionId) : IRequest;

public sealed class CompleteMissionCommandHandler : IRequestHandler<CompleteMissionCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;

    private readonly IUserRepository _userRepository;

    public CompleteMissionCommandHandler(IUser user, IUserRepository userRepository, IApplicationDbContext dbContext)
    {
        _user = user;
        _userRepository = userRepository;
        _dbContext = dbContext;
    }

    public async Task Handle(CompleteMissionCommand request, CancellationToken cancellationToken)
    {
        IDomainUser? user = await _userRepository.GetById(_user.Id ?? string.Empty);

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }

        Mission? mission = _dbContext.Missions.FirstOrDefault(m => m.Id == request.MissionId && m.IsActive);

        if (mission is null)
        {
            throw new NotFoundException(request.MissionId.ToString(), nameof(Mission));
        }

        MissionService.CompleteMission(user, mission);

        await _userRepository.Update(user);
    }
}
