using System.Data;
using Dapper;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Homepage.Models;
using DashiToon.Api.Application.Homepage.Queries.GetRecentlyUpdatedSeries;
using DashiToon.Api.Application.Homepage.Queries.GetRecommendSeries;
using DashiToon.Api.Infrastructure.Data;

namespace DashiToon.Api.Infrastructure.Repositories;

public class HomepageRepository : IHomepageRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public HomepageRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<RecentlyUpdatedSeriesDto>> GetRecentlyUpdatedSeries()
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.QueryAsync<RecentlyUpdatedSeriesDto>(
            """
            WITH SeriesGenres AS (SELECT "Series"."Id",
                                         jsonb_agg(G2."Name") AS "Genres"
                                  FROM "Series"
                                           JOIN public."GenreSeries" GS2 on "Series"."Id" = GS2."SeriesId"
                                           JOIN public."Genres" G2 on GS2."GenresId" = G2."Id"
                                  GROUP BY "Series"."Id"),
                 SeriesRatings AS (SELECT "Series"."Id",
                                          (CAST(sum(Case WHEN "Reviews"."IsRecommended" = TRUE THEN 1 ELSE 0 END) AS DECIMAL) *
                                           100 /
                                           count(*)) AS "Rating"
                                   FROM "Series"
                                            LEFT JOIN "Reviews" ON "Series"."Id" = "Reviews"."SeriesId"
                                   GROUP BY "Series"."Id")
            SELECT s."Id",
                   s."Title",
                   s."AlternativeTitles",
                   s."Authors",
                   SeriesRatings."Rating",
                   s."Status",
                   s."Thumbnail",
                   s."Type",
                   SeriesGenres."Genres",
                   s."ContentRating",
                   c."PublishedDate",
                   c."Id"     AS "ChapterId",
                   c."ChapterNumber",
                   cv."Title" AS "ChapterName",
                   c."PublishedDate"
            FROM "Series" s
                     LEFT JOIN SeriesGenres ON s."Id" = SeriesGenres."Id"
                     LEFT JOIN SeriesRatings ON s."Id" = SeriesRatings."Id"
                     JOIN "Volumes" v ON v."SeriesId" = s."Id"
                     JOIN "Chapters" c ON c."VolumeId" = v."Id"
                     JOIN "ChapterVersion" cv on c."PublishedVersionId" = cv."Id"
            WHERE c."PublishedDate" >= NOW() - INTERVAL '1 Day'
              AND c."PublishedDate" <= NOW()
              AND s."Status" NOT IN (0, 2, 4)
            ORDER BY c."PublishedDate" DESC;
            """);
    }

    public async Task<IEnumerable<SeriesInfoDto>> GetSeries(int[] seriesIds, int pageNumber = 1, int pageSize = 12)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.QueryAsync<SeriesInfoDto>(
            """
            WITH SeriesGenres AS (SELECT "Series"."Id",
                                         jsonb_agg(G2."Name") AS "Genres"
                                  FROM "Series"
                                           JOIN public."GenreSeries" GS2 on "Series"."Id" = GS2."SeriesId"
                                           JOIN public."Genres" G2 on GS2."GenresId" = G2."Id"
                                  GROUP BY "Series"."Id"),
                 SeriesRatings AS (SELECT "Series"."Id",
                                          (CAST(sum(Case WHEN "Reviews"."IsRecommended" = TRUE THEN 1 ELSE 0 END) AS DECIMAL) *
                                           100 /
                                           count(*)) AS "Rating"
                                   FROM "Series"
                                            LEFT JOIN "Reviews" ON "Series"."Id" = "Reviews"."SeriesId"
                                   GROUP BY "Series"."Id")
            SELECT s."Id",
                   s."Title",
                   s."AlternativeTitles",
                   s."Authors",
                   SeriesRatings."Rating",
                   s."Status",
                   s."Thumbnail",
                   s."Type",
                   SeriesGenres."Genres",
                   s."ContentRating"
            FROM "Series" s
                     LEFT JOIN SeriesGenres ON s."Id" = SeriesGenres."Id"
                     LEFT JOIN SeriesRatings ON s."Id" = SeriesRatings."Id"
            WHERE s."Id" = ANY(@SeriesIds) AND s."Status" NOT IN (0, 2, 4)
            LIMIT @Limit OFFSET @Offset;
            """,
            new { SeriesIds = seriesIds, Limit = pageSize, Offset = (pageNumber - 1) * pageSize }
        );
    }

    public async Task<IEnumerable<SeriesInfoDto>> GetRecentlyReleasedSeries()
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.QueryAsync<SeriesInfoDto>(
            """
            WITH SeriesGenres AS (SELECT "Series"."Id",
                                         jsonb_agg(G2."Name") AS "Genres"
                                  FROM "Series"
                                           JOIN public."GenreSeries" GS2 on "Series"."Id" = GS2."SeriesId"
                                           JOIN public."Genres" G2 on GS2."GenresId" = G2."Id"
                                  GROUP BY "Series"."Id"),
                 SeriesRatings AS (SELECT "Series"."Id",
                                          (CAST(sum(Case WHEN "Reviews"."IsRecommended" = TRUE THEN 1 ELSE 0 END) AS DECIMAL) *
                                           100 /
                                           count(*)) AS "Rating"
                                   FROM "Series"
                                            LEFT JOIN "Reviews" ON "Series"."Id" = "Reviews"."SeriesId"
                                   GROUP BY "Series"."Id")
            SELECT s."Id",
                   s."Title",
                   s."AlternativeTitles",
                   s."Authors",
                   SeriesRatings."Rating",
                   s."Status",
                   s."Thumbnail",
                   s."Type",
                   SeriesGenres."Genres",
                   s."ContentRating",
                   s."StartTime"
            FROM "Series" s
                     LEFT JOIN SeriesGenres ON s."Id" = SeriesGenres."Id"
                     LEFT JOIN SeriesRatings ON s."Id" = SeriesRatings."Id"
            WHERE "StartTime" IS NOT NULL
              AND "Status" NOT IN (0, 2, 4)
              AND "StartTime" >= now() - INTERVAL '30 Day'
              AND "StartTime" <= now() + INTERVAL '1 Day'
            ORDER BY "StartTime" DESC
            LIMIT 12;
            """
        );
    }

    public async Task<IEnumerable<SeriesInfoDto>> GetTrendingGenreSeries(int genreId)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.QueryAsync<SeriesInfoDto>(
            """
            WITH SeriesGenres AS (SELECT "Series"."Id",
                                         jsonb_agg(G2."Name") AS "Genres"
                                  FROM "Series"
                                           JOIN public."GenreSeries" GS2 on "Series"."Id" = GS2."SeriesId"
                                           JOIN public."Genres" G2 on GS2."GenresId" = G2."Id"
                                  GROUP BY "Series"."Id"),
                 SeriesRatings AS (SELECT "Series"."Id",
                                          (CAST(sum(Case WHEN "Reviews"."IsRecommended" = TRUE THEN 1 ELSE 0 END) AS DECIMAL) *
                                           100 /
                                           count(*)) AS "Rating"
                                   FROM "Series"
                                            LEFT JOIN "Reviews" ON "Series"."Id" = "Reviews"."SeriesId"
                                   GROUP BY "Series"."Id")
            SELECT s."Id",
                   s."Title",
                   s."AlternativeTitles",
                   s."Authors",
                   SeriesRatings."Rating",
                   s."Status",
                   s."Thumbnail",
                   s."Type",
                   SeriesGenres."Genres",
                   s."ContentRating",
                   s."StartTime"
            FROM "Series" s
                     LEFT JOIN SeriesGenres ON s."Id" = SeriesGenres."Id"
                     LEFT JOIN SeriesRatings ON s."Id" = SeriesRatings."Id"
            WHERE "StartTime" IS NOT NULL
              AND "Status" NOT IN (0, 2, 4)
              AND "StartTime" >= now() - INTERVAL '30 Days'
              AND "StartTime" <= now()
            ORDER BY "StartTime" DESC
            LIMIT 12;
            """,
            new { GenreId = genreId }
        );
    }

    public async Task<IEnumerable<UserLikedSeriesDto>> GetUserLikedSeries()
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.QueryAsync<UserLikedSeriesDto>(
            """
            SELECT 
                r."UserId", 
                ARRAY_AGG(r."SeriesId") AS "SeriesArray"
            FROM 
                "Reviews" r
            JOIN 
                "Users" u 
            ON 
                r."UserId" = u."Id"
            WHERE 
                r."IsRecommended" = TRUE
            GROUP BY 
                r."UserId";
            """
        );
    }
}
