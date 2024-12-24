using System.Data;
using Dapper;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Users.Queries.GetFollowedSeries;
using DashiToon.Api.Application.Users.Queries.GetSeriesFollowDetail;
using DashiToon.Api.Infrastructure.Data;

namespace DashiToon.Api.Infrastructure.Repositories;

public class FollowRepository : IFollowRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public FollowRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<(int Count, IEnumerable<FollowedSeriesDto> Result)> GetFollowedSeries(
        string userId,
        bool? hasRead,
        string sortBy,
        string sortOrder,
        int pageNumber,
        int pageSize)
    {
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync();

        int count = await connection.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(*)
            FROM "Follows"
            WHERE "UserId" = @UserId
            AND (@HasRead IS NULL OR "Follows"."LatestChapterId" IS NOT NULL = @HasRead);
            """,
            new { UserId = userId, HasRead = hasRead }
        );

        string sort = sortBy.ToLower() switch
        {
            "lastread" => "F.\"Timestamp\"",
            _ => "S.\"Title\""
        };

        string order = sortOrder.ToLower() switch
        {
            "asc" => "ASC",
            _ => "DESC"
        };

        string orderStatement = $"ORDER BY {sort} {order}";

        IEnumerable<FollowedSeriesDto> result = await connection.QueryAsync<FollowedSeriesDto>(
            $"""
             WITH ChapterInfo AS (SELECT V."VolumeNumber",
                                         C."ChapterNumber",
                                         C."Id",
                                         V."SeriesId"
                                  FROM "Chapters" C
                                           JOIN "Volumes" V ON C."VolumeId" = V."Id")
             SELECT S."Thumbnail",
                    S."Title",
                    S."Type",
                    S."Status",
                    S."Id" AS "SeriesId",
                    V."Id" AS "VolumeId",
                    F."LatestChapterId",
                    F."IsNotified",
                    (
                        CASE
                            WHEN F."LatestChapterId" IS NULL THEN 0
                            ELSE (SELECT count(*)
                                  FROM ChapterInfo CI
                                  WHERE CI."SeriesId" = F."SeriesId"
                                    AND (CI."VolumeNumber" < V."VolumeNumber"
                                      OR (CI."VolumeNumber" = V."VolumeNumber" AND
                                          CI."ChapterNumber" <= C."ChapterNumber"))) END) AS "Progress",
                    (SELECT count(*)
                     FROM ChapterInfo CI
                     WHERE CI."SeriesId" = F."SeriesId")                                  AS "TotalChapters"
             FROM "Follows" F
                      JOIN "Series" S ON F."SeriesId" = S."Id"
                      LEFT JOIN "Chapters" C ON F."LatestChapterId" = C."Id"
                      LEFT JOIN "Volumes" V ON C."VolumeId" = V."Id"
             WHERE F."UserId" = @UserId 
             AND (@HasRead IS NULL OR F."LatestChapterId" IS NOT NULL = @HasRead)
             {orderStatement}
             LIMIT @Limit OFFSET @Offset;
             """,
            new { UserId = userId, HasRead = hasRead, Limit = pageSize, Offset = pageSize * (pageNumber - 1) }
        );

        return new ValueTuple<int, IEnumerable<FollowedSeriesDto>>(count, result);
    }

    public async Task<FollowDetailDto?> GetFollowedSeriesById(string userId, int seriesId)
    {
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync();

        return await connection.QuerySingleOrDefaultAsync<FollowDetailDto>(
            """
            WITH ChapterInfo AS (SELECT V."VolumeNumber",
                                        C."ChapterNumber",
                                        C."Id",
                                        V."SeriesId"
                                 FROM "Chapters" C
                                          JOIN "Volumes" V ON C."VolumeId" = V."Id"
                                 WHERE V."SeriesId" = @SeriesId)
            SELECT S."Id"                                                                AS "SeriesId",
                   V."Id"                                                                AS "VolumeId",
                   F."LatestChapterId",
                   F."IsNotified",
                   (
                       CASE
                           WHEN F."LatestChapterId" IS NULL THEN 0
                           ELSE (SELECT count(*)
                                 FROM ChapterInfo CI
                                 WHERE CI."SeriesId" = F."SeriesId"
                                   AND (CI."VolumeNumber" < V."VolumeNumber"
                                     OR (CI."VolumeNumber" = V."VolumeNumber" AND
                                         CI."ChapterNumber" <= C."ChapterNumber"))) END) AS "Progress",
                   (SELECT count(*)
                    FROM ChapterInfo CI
                    WHERE CI."SeriesId" = F."SeriesId")                                  AS "TotalChapters"
            FROM "Follows" F
                     JOIN "Series" S ON F."SeriesId" = S."Id"
                     LEFT JOIN "Chapters" C ON F."LatestChapterId" = C."Id"
                     LEFT JOIN "Volumes" V ON C."VolumeId" = V."Id"
            WHERE F."SeriesId" = @SeriesId
              AND F."UserId" = @UserId
            """,
            new { UserId = userId, SeriesId = seriesId }
        );
    }
}
