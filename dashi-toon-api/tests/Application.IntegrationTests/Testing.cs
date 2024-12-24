using System.Data;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Common;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Infrastructure.Data;
using DashiToon.Api.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application.IntegrationTests;

public class Testing : IAsyncLifetime
{
    private static ITestDatabase? _database;
    private static IntegrationTestWebAppFactory _factory = null!;
    private static IServiceScopeFactory _scopeFactory = null!;
    private static string? _userId;
    private static string? _userName;

    public async Task InitializeAsync()
    {
        _database = await TestDatabaseFactory.CreateAsync();

        _factory = new IntegrationTestWebAppFactory(_database.GetConnection());

        _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    public async Task DisposeAsync()
    {
        await _database!.DisposeAsync();
        await _factory.DisposeAsync();
    }

    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();

        ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        return await mediator.Send(request);
    }

    public static async Task SendAsync(IBaseRequest request)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();

        ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        await mediator.Send(request);
    }

    public static string? GetUserId()
    {
        return _userId;
    }

    public static string? GetUserName()
    {
        return _userName;
    }

    public static async Task<string> RunAsDefaultUserAsync()
    {
        return await RunAsUserAsync("test@local", "Testing1234!", []);
    }

    public static async Task<string> RunAsModeratorAsync()
    {
        return await RunAsUserAsync("moderator@local", "Moderator1234!", [Roles.Moderator]);
    }

    public static async Task<string> RunAsAdministratorAsync()
    {
        return await RunAsUserAsync("administrator@local", "Administrator1234!", [Roles.Administrator]);
    }

    public static async Task<string> RunAsUserAsync(string userName, string password, string[] roles)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();

        UserManager<ApplicationUser> userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        ApplicationUser? user = await userManager.FindByNameAsync(userName);

        if (user != null)
        {
            _userId = user.Id;
            _userName = user.UserName;

            return user.Id;
        }

        user = new ApplicationUser { UserName = userName, Email = userName };

        IdentityResult result = await userManager.CreateAsync(user, password);

        if (roles.Any())
        {
            RoleManager<IdentityRole> roleManager =
                scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (string role in roles)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }

            await userManager.AddToRolesAsync(user, roles);
        }

        if (result.Succeeded)
        {
            _userId = user.Id;
            _userName = user.UserName;

            return _userId;
        }

        string errors = string.Join(Environment.NewLine, result.ToApplicationResult().Errors);

        throw new Exception($"Unable to create {userName}.{Environment.NewLine}{errors}");
    }

    public static void LogOut()
    {
        _userId = null;
        _userName = null;
    }

    public static async Task ResetState()
    {
        try
        {
            await _database!.ResetAsync();
        }
        catch (Exception)
        {
            // ignored
        }

        _userId = null;
    }

    public static async Task<TEntity?> FindAsync<TEntity>(params object[] keyValues)
        where TEntity : class
    {
        using IServiceScope scope = _scopeFactory.CreateScope();

        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.FindAsync<TEntity>(keyValues);
    }

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using IServiceScope scope = _scopeFactory.CreateScope();

        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.AddAsync(entity);

        await context.SaveChangesAsync();
    }

    public static async Task<int> CountAsync<TEntity>()
        where TEntity : class
    {
        using IServiceScope scope = _scopeFactory.CreateScope();

        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().CountAsync();
    }

    public static IDbConnection GetConnection()
    {
        IServiceScope scope = _scopeFactory.CreateScope();

        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return context.Database.GetDbConnection();
    }

    public static IApplicationDbContext GetContext()
    {
        IServiceScope scope = _scopeFactory.CreateScope();

        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return context;
    }
}
