using System.Data.Common;
using DashiToon.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Respawn;

namespace Application.IntegrationTests;

public class PostgresServerTestDatabase : ITestDatabase
{
    private readonly string _connectionString = null!;
    private NpgsqlConnection _connection = null!;
    private Respawner _respawner = null!;

    public PostgresServerTestDatabase()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        string? connectionString = configuration.GetConnectionString("DefaultConnection");

        Guard.Against.Null(connectionString);

        _connectionString = connectionString;
    }


    public async Task InitializeAsync()
    {
        _connection = new NpgsqlConnection(_connectionString);

        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        ApplicationDbContext context = new(options);

        await context.Database.MigrateAsync();

        _respawner = await Respawner.CreateAsync(_connectionString,
            new RespawnerOptions { TablesToIgnore = ["__EFMigrationsHistory"], DbAdapter = DbAdapter.Postgres });
    }

    public DbConnection GetConnection()
    {
        return _connection;
    }

    public async Task ResetAsync()
    {
        await _respawner.ResetAsync(_connectionString);
    }

    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
