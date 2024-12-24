using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Services;

namespace DashiToon.Api.Application.Users.Commands.CheckinUser;

[Authorize]
public sealed record CheckinUserCommand : IRequest;

public sealed class CheckinUserCommandHandler : IRequestHandler<CheckinUserCommand>
{
    private readonly IUser _user;

    private readonly IUserRepository _userRepository;

    public CheckinUserCommandHandler(IUser user, IUserRepository userRepository)
    {
        _user = user;
        _userRepository = userRepository;
    }

    public async Task Handle(CheckinUserCommand request, CancellationToken cancellationToken)
    {
        IDomainUser? user = await _userRepository.GetById(_user.Id ?? string.Empty);

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }

        MissionService.DailyCheckin(user);

        await _userRepository.Update(user);
    }
}
