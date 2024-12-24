using System.Data;
using Npgsql;

namespace DashiToon.Api.Infrastructure.Data;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync();
}

public class NpgsqlConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public async Task<IDbConnection> CreateConnectionAsync()
    {
        NpgsqlConnection connection = new(connectionString);

        await connection.OpenAsync();

        return connection;
    }
}
