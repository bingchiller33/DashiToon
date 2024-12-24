using System.Security.Claims;
using DashiToon.Api.Application.Common.Interfaces;

namespace DashiToon.Api.Web.Services;

public class CurrentUser : IUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? Id => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Name => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
}
