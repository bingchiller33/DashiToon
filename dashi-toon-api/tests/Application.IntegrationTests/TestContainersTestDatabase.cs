using System.Data.Common;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace Application.IntegrationTests;

public class TestContainersTestDatabase : ITestDatabase
{
    private readonly PostgreSqlContainer _container;
    private DbConnection _connection = null!;
    private string _connectionString = null!;
    private Respawner _respawner = null!;

    public TestContainersTestDatabase()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithUsername("postgres")
            .WithPassword("DashiToon@123!")
            .WithAutoRemove(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        _connectionString = _container.GetConnectionString();

        _connection = new NpgsqlConnection(_connectionString);

        await _connection.OpenAsync();

        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        ApplicationDbContext context = new(options);

        await context.Database.MigrateAsync();

        _respawner = await Respawner.CreateAsync(_connection,
            new RespawnerOptions
            {
                TablesToIgnore =
                    ["__EFMigrationsHistory", "Genres", "Roles", "KanaExchangeRates", "CommissionRates"],
                DbAdapter = DbAdapter.Postgres
            });
    }

    public DbConnection GetConnection()
    {
        return _connection;
    }

    public async Task ResetAsync()
    {
        await _respawner.ResetAsync(_connection);
    }

    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _container.DisposeAsync();
    }
}
