using System.Data;
using Dapper;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Infrastructure.Data;

namespace DashiToon.Api.Infrastructure.Repositories;

public class VolumeRepository : IVolumeRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public VolumeRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Volume>> FindVolumesBySeriesId(int seriesId)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.QueryAsync<Volume>(
            """
            SELECT *
            FROM "Volumes"
            WHERE "SeriesId" = @SeriesId
            """,
            new { SeriesId = seriesId });
    }
}
