using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Moderation.Model;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Services;

namespace DashiToon.Api.Application.Moderation.Queries.IsAuthorAllowedToPublish;

[Authorize]
public sealed record IsAuthorAllowedToPublishQuery : IRequest<AllowVm>;

public sealed class IsAuthorAllowedToPublishQueryHandler : IRequestHandler<IsAuthorAllowedToPublishQuery, AllowVm>
{
    private readonly IUserRepository _userRepository;
    private readonly IUser _user;

    public IsAuthorAllowedToPublishQueryHandler(IUserRepository userRepository, IUser user)
    {
        _userRepository = userRepository;
        _user = user;
    }

    public async Task<AllowVm> Handle(IsAuthorAllowedToPublishQuery request, CancellationToken cancellationToken)
    {
        IDomainUser? user = await _userRepository.GetUserById(_user.Id!);

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }

        bool isAllowed = ReportService.IsUserAllowedToPublish(user);

        return isAllowed
            ? new AllowVm(isAllowed)
            : new AllowVm(isAllowed, user.RestrictPublishUntil!.Value.ToString("O"));
    }
}
