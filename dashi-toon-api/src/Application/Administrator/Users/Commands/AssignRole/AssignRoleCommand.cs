using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;

namespace DashiToon.Api.Application.Administrator.Users.Commands.AssignRole;

[Authorize(Roles = Roles.Administrator)]
public sealed record AssignRoleCommand(
    string UserId,
    string Role) : IRequest<Result>;

public sealed class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, Result>
{
    private readonly IIdentityService _identityService;

    public AssignRoleCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.AssignRole(request.UserId, request.Role);
    }
}
