using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;

namespace DashiToon.Api.Application.Administrator.Users.Queries.GetRoles;

[Authorize(Roles = Roles.Administrator)]
public sealed record GetRolesQuery : IRequest<List<string>>;

public sealed class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, List<string>>
{
    private readonly IIdentityService _identityService;

    public GetRolesQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<List<string>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        return _identityService.GetRolesAsync();
    }
}
