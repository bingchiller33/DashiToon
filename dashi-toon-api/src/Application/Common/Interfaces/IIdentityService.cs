using DashiToon.Api.Application.Administrator.Users.Models;
using DashiToon.Api.Application.Common.Models;

namespace DashiToon.Api.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);
    Task<bool> IsInOneOfRolesAsync(string userId, params string[] roles);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

    Task<Result> DeleteUserAsync(string userId);

    Task<List<string>> GetRolesAsync();
    Task<List<UserVm>> GetUsersAsync();
    Task<Result> AssignRole(string userId, string role);
}
