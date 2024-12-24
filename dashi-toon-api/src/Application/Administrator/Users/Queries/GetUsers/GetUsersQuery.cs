using DashiToon.Api.Application.Administrator.Users.Models;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;

namespace DashiToon.Api.Application.Administrator.Users.Queries.GetUsers;

[Authorize(Roles = Roles.Administrator)]
public sealed record GetUsersQuery(
    string? UserId,
    string? UserName,
    string? Role,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PaginatedList<UserVm>>;

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedList<UserVm>>
{
    private readonly IIdentityService _identityService;

    public GetUsersQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<PaginatedList<UserVm>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        List<UserVm>? users = await _identityService.GetUsersAsync();

        List<UserVm>? result = users.Where(u =>
                (request.UserId is null || u.UserId == request.UserId)
                && (request.UserName is null || u.UserName == request.UserName)
                && (request.Role is null || u.Roles.Contains(request.Role)))
            .ToList();

        return new PaginatedList<UserVm>(
            result
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList(),
            result.Count,
            request.PageNumber,
            request.PageSize
        );
    }
}
