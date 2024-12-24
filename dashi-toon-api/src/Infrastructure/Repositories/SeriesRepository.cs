using System.Data;
using Dapper;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.SeriesDetail.Queries.GetRelatedSeries;
using DashiToon.Api.Application.SeriesDetail.Queries.GetSeriesAnalytics;
using DashiToon.Api.Domain.Enums;
using DashiToon.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DashiToon.Api.Infrastructure.Repositories;

public class SeriesRepository : ISeriesRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IApplicationDbContext _context;

    public SeriesRepository(IDbConnectionFactory connectionFactory, IApplicationDbContext context)
    {
        _connectionFactory = connectionFactory;
        _context = context;
    }

    public async Task<Domain.Entities.Series?> FindSeriesById(int seriesId)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.QueryFirstOrDefaultAsync<Domain.Entities.Series>(
            """
            SELECT * FROM "Series"
            where "Id" = @Id
            """,
            new { Id = seriesId });
    }

    public async Task<Domain.Entities.Series?> FindSeriesWithVolumesAndChaptersById(int seriesId)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        return await _context.Series
            .Include(s => s.Volumes)
            .ThenInclude(v => v.Chapters)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Id == seriesId
                                      && s.Status != SeriesStatus.Draft
                                      && s.Status != SeriesStatus.Trashed
                                      && s.Status != SeriesStatus.Hiatus);
    }

    public async Task<IEnumerable<RelatedSeriesDto>> GetRelatedSeries(int seriesId)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.QueryAsync<RelatedSeriesDto>(
            """
            WITH SeriesRatings AS (SELECT "Series"."Id",
                                          (CAST(sum(Case WHEN "Reviews"."IsRecommended" = TRUE THEN 1 ELSE 0 END) AS DECIMAL) *
                                           100 /
                                           count(*)) AS "Rating"
                                   FROM "Series"
                                            LEFT JOIN "Reviews" ON "Series"."Id" = "Reviews"."SeriesId"
                                   GROUP BY "Series"."Id"),
                 genres_vector AS (SELECT s."Id"   AS series_id,
                                          g."Id"   AS genre_id,
                                          g."Name" AS genre_name,
                                          CASE
                                              WHEN gs."GenresId" IS NOT NULL THEN 1
                                              ELSE 0
                                              END  AS is_present
                                   FROM "Series" s
                                            CROSS JOIN
                                            (SELECT "Id", "Name" FROM "Genres") g
                                            LEFT JOIN
                                        "GenreSeries" gs ON s."Id" = gs."SeriesId" AND g."Id" = gs."GenresId")
            SELECT s."Id",
                   s."Title",
                   s."AlternativeTitles",
                   s."Authors",
                   SR."Rating",
                   s."Thumbnail",
                   s."Type",
                   s."Status",
                   s."ContentRating",
                   s."StartTime",
                   array_agg(gv.is_present ORDER BY gv.genre_id)             AS GenresMap,
                   jsonb_agg(gv.genre_name) FILTER (WHERE gv.is_present = 1) as Genres
            FROM "Series" s
                     JOIN genres_vector gv ON s."Id" = gv.series_id
                     JOIN SeriesRatings SR ON s."Id" = SR."Id"
            WHERE s."Status" NOT IN (0, 2, 4)
            GROUP BY s."Id", SR."Rating", s."Title", s."AlternativeTitles", s."Authors", s."Id", s."Thumbnail", s."Type",
                     s."Status", s."ContentRating", s."StartTime"
            ORDER BY CASE WHEN s."Id" = @Id THEN 0 ELSE 1 END, s."Id";
            """,
            new { Id = seriesId });
    }

    public async Task<SeriesAnalyticsDto> GetSeriesAnalytics(int seriesId)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.QueryFirstAsync<SeriesAnalyticsDto>(
            """
            SELECT
            (SELECT count(*)
             FROM "Reviews"
             WHERE "SeriesId" = @SeriesId
               AND "IsRecommended" = true) AS "RecommendedCount",

            (SELECT count(*)
             FROM "Reviews"
             WHERE "SeriesId" = @SeriesId) AS "ReviewCount",

            (SELECT count(*)
             FROM "Follows"
             WHERE "SeriesId" = @SeriesId) AS "FollowCount",

            (SELECT sum("ViewCount")
             FROM "Chapters"
                      JOIN "Volumes" ON "Chapters"."VolumeId" = "Volumes"."Id"
             WHERE "Volumes"."SeriesId" = @SeriesId) AS "ViewCount",

            (SELECT "LastModified"
             FROM "Series"
             WHERE "Id" = @SeriesId) AS "LastModified"
            """,
            new { SeriesId = seriesId }
        );
    }

    public async Task<double> GetSeriesRating(int seriesId)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.ExecuteScalarAsync<double>(
            """
            SELECT (SUM(CASE WHEN "IsRecommended" THEN 1 ELSE 0 END) * 100.0) / COUNT(*) AS "RecommendedPercentage"
            FROM "Reviews"
            WHERE "SeriesId" = @SeriesId;
            """,
            new { SeriesId = seriesId }
        );
    }
}
