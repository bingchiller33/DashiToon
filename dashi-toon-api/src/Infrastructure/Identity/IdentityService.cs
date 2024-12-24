using System.Security.Claims;
using DashiToon.Api.Application.Administrator.Users.Models;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Models;
using Elastic.Clients.Elasticsearch.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DashiToon.Api.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public IdentityService(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
    {
        ApplicationUser user = new() { UserName = userName, Email = userName };

        IdentityResult result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> IsInOneOfRolesAsync(string userId, params string[] roles)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        foreach (string role in roles)
        {
            if (await _userManager.IsInRoleAsync(user, role))
            {
                return true;
            }
        }

        return false;
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        ClaimsPrincipal principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        AuthorizationResult result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        IdentityResult result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }

    public async Task<List<string>> GetRolesAsync()
    {
        List<string>? roles = await _roleManager.Roles
            .Select(r => r.NormalizedName ?? string.Empty)
            .ToListAsync();

        return roles;
    }

    public async Task<List<UserVm>> GetUsersAsync()
    {
        List<ApplicationUser>? users = await _userManager.Users.ToListAsync();

        List<UserVm>? result = new();

        foreach (ApplicationUser? user in users)
        {
            IList<string>? roles = await _userManager.GetRolesAsync(user);

            result.Add(new UserVm(
                user.Id,
                user.UserName,
                user.Email,
                user.PhoneNumber,
                roles
            ));
        }

        return result;
    }

    public async Task<Result> AssignRole(string userId, string role)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return Result.Failure(["User not found"]);
        }

        IdentityRole? userRole = await _roleManager.FindByNameAsync(role);

        if (userRole is null)
        {
            return Result.Failure(["Role not found"]);
        }

        IdentityResult? result = await _userManager.AddToRoleAsync(user, role);

        return result.ToApplicationResult();
    }
}
