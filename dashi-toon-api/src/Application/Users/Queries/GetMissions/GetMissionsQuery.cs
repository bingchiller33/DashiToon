using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Users.Queries.GetMissions;

[Authorize]
public sealed record GetMissionsQuery : IRequest<List<MissionVm>>;

public sealed class GetMissionsQueryHandler : IRequestHandler<GetMissionsQuery, List<MissionVm>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;
    private readonly IUserRepository _userRepository;

    public GetMissionsQueryHandler(IUser user, IUserRepository userRepository, IApplicationDbContext dbContext)
    {
        _user = user;
        _userRepository = userRepository;
        _dbContext = dbContext;
    }

    public async Task<List<MissionVm>> Handle(GetMissionsQuery request, CancellationToken cancellationToken)
    {
        IDomainUser? user = await _userRepository.GetById(_user.Id ?? string.Empty);

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }

        List<Mission> missions = await _dbContext.Missions
            .OrderByDescending(m => m.ReadCount)
            .ToListAsync(cancellationToken);

        return missions
            .Where(m => m.IsActive)
            .Select(m => new MissionVm(
                m.Id,
                m.Reward,
                m.ReadCount,
                user.IsMissionCompleted(m),
                user.IsMissionCompletable(m)
            ))
            .ToList();
    }
}
