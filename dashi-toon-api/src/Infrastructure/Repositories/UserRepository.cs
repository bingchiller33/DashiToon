using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DashiToon.Api.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IDomainUser?> GetUserById(string id)
    {
        return await _userManager.FindByIdAsync(id);
    }

    public async Task<IDomainUser?> GetById(string id)
    {
        return await _userManager.Users
            .Include(u => u.Histories)
            .Include(u => u.MissionCompletions)
            .Include(u => u.Subscriptions).ThenInclude(s => s.Tier)
            .Include(u => u.PurchaseOrders)
            .Include(u => u.Ledgers)
            .Include(u => u.UnlockedChapters)
            .Include(u => u.Follows)
            .AsSplitQuery()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<string?> GetUsernameById(string id)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(id);
        return user?.UserName;
    }

    public async Task<string?> GetUserAvatarById(string id)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(id);
        return user?.Avatar;
    }

    public async Task Update(IDomainUser user)
    {
        await _userManager.UpdateAsync((ApplicationUser)user);
    }
}
