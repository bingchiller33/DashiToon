using System.Data;
using Dapper;
using DashiToon.Api.Application.AuthorStudio.Analytics.Models;
using DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesDashiFanRankings;
using DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesGeneralAnalytis;
using DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesReviewAnalytics;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Genres.Queries.GetGenres;
using DashiToon.Api.Domain.Enums;
using DashiToon.Api.Infrastructure.Data;
using Elastic.Clients.Elasticsearch.Snapshot;

namespace DashiToon.Api.Infrastructure.Repositories;

public class AnalyticRepository : IAnalyticRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AnalyticRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<GeneralAnalyticsDto> GetGeneralAnalyticsAsync(DateRange range, int seriesId)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.QueryFirstAsync<GeneralAnalyticsDto>(
            """
            SELECT
            (SELECT sum("ReadingAnalytic"."ViewCount")
             FROM "ReadingAnalytic"
                      JOIN public."Chapters" C ON C."Id" = "ReadingAnalytic"."ChapterId"
                      JOIN "Volumes" V ON C."VolumeId" = V."Id"
             WHERE "Timestamp" >= @From
               AND "Timestamp" <= @To
               AND "SeriesId" = @SeriesId)                AS "ViewCount",

            (SELECT (CAST("RecommendedCount" AS DECIMAL) / "ReviewCount") * 100
             FROM (SELECT sum(CASE WHEN "IsRecommended" THEN 1 ELSE 0 END) AS "RecommendedCount",
                          count(*)                                         AS "ReviewCount"
                   FROM "Reviews"
                   WHERE "Timestamp" >= @From
                     AND "Timestamp" <= @To
                     AND "SeriesId" = @SeriesId) as ReviewAnalytic) AS "Rating",

            (SELECT sum("Amount") AS "Revenue"
             FROM "RevenueTransaction"
                      JOIN public."Series" S ON S."Id" = "RevenueTransaction"."SeriesId"
             WHERE "Timestamp" >= @From
               AND "Timestamp" <= @To
               AND "SeriesId" = @SeriesId)                AS "Revenue",

            (SELECT count(DISTINCT SH."SubscriptionId") AS "DashiFanCount"
             FROM "SubscriptionHistory" SH
                      JOIN "Subscriptions" S ON S."Id" = SH."SubscriptionId"
                      JOIN "DashiFans" DF ON DF."Id" = S."DashiFanId"
             WHERE SH."Status" = @Status
               AND (SH."Timestamp" + INTERVAL '1 month') >= @From
               AND SH."Timestamp" <= @To
               AND "SeriesId" = @SeriesId)                AS "DashiFanCount"
            """,
            new { range.From, range.To, SeriesId = seriesId, Status = SubscriptionStatus.Active }
        );
    }


    public async Task<int> GetSeriesViewRankingAsync(DateRange range, int seriesId)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        int result = await connection.ExecuteScalarAsync<int>(
            """
            SELECT "Rank"
            FROM (SELECT S."Id"                                                                                           AS "SeriesId",
                         sum(CASE WHEN RA."ViewCount" is null THEN 0 ELSE RA."ViewCount" END)                             AS "View",
                         rank() over (ORDER BY sum(Case when RA."ViewCount" is null then 0 else RA."ViewCount" END) DESC) AS "Rank"
                  FROM "Series" S
                           left join "Volumes" V on S."Id" = V."SeriesId"
                           left join "Chapters" C on V."Id" = C."VolumeId"
                           left join "ReadingAnalytic" RA on C."Id" = RA."ChapterId"
                  WHERE RA."Timestamp" >= @From
                    AND RA."Timestamp" <= @To
                  GROUP BY S."Id"
                  ORDER BY "View" DESC) AS "Rankings"
            WHERE "Rankings"."SeriesId" = @SeriesId
            """,
            new { range.From, range.To, SeriesId = seriesId }
        );

        return result;
    }

    public async Task<int> GetSeriesViewCountAsync(DateRange range, int seriesId)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.ExecuteScalarAsync<int>(
            """
            SELECT sum(CASE when RA."ViewCount" is null then 0 else RA."ViewCount" END) AS "ViewCount"
            FROM "ReadingAnalytic" RA
                     RIGHT JOIN public."Chapters" C ON C."Id" = RA."ChapterId"
                     RIGHT JOIN public."Volumes" V ON C."VolumeId" = V."Id"
            WHERE RA."Timestamp" >= @From
              AND RA."Timestamp" <= @To
              AND "SeriesId" = @SeriesId;
            """,
            new { range.From, range.To, SeriesId = seriesId }
        );
    }

    public async Task<IEnumerable<ChartDataDto>> GetSeriesViewCountBreakdownInDayAsync(DateRange range, int seriesId)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.QueryAsync<ChartDataDto>(
            """
            WITH days AS (SELECT generate_series(@From::date, @To::date, '1 day'::interval)::date AS "Date")
            SELECT to_char(d."Date", 'DD-MM') AS "Time",
                   COALESCE(SUM(agg."ViewCount"), 0) AS "Data"
            FROM days d
                     LEFT JOIN (SELECT DATE(RA."Timestamp") AS "Date",
                                       SUM(RA."ViewCount")  AS "ViewCount"
                                FROM "ReadingAnalytic" RA
                                         JOIN public."Chapters" C ON C."Id" = RA."ChapterId"
                                         JOIN public."Volumes" V ON C."VolumeId" = V."Id"
                                WHERE RA."Timestamp" >= @From
                                  AND RA."Timestamp" <= @To
                                  AND V."SeriesId" = @SeriesId
                                GROUP BY "Date") AS agg ON d."Date" = agg."Date"
            GROUP BY d."Date"
            ORDER BY d."Date"
            """,
            new { range.From, range.To, SeriesId = seriesId }
        );
    }


    public async Task<IEnumerable<ChartDataDto>> GetSeriesViewCountBreakdownInWeekAsync(DateRange range, int seriesId)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.QueryAsync<ChartDataDto>(
            """
            WITH days AS (SELECT unnest(ARRAY ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday']) AS "DayName")
            SELECT d."DayName" AS "Time",
                   COALESCE(SUM(agg."ViewCount"), 0) AS "Data"
            FROM days d
                     LEFT JOIN (SELECT TO_CHAR(RA."Timestamp", 'Day') AS "DayName",
                                       SUM(Case when RA."ViewCount" is null then 0 else RA."ViewCount" END)            AS "ViewCount"
                                FROM "ReadingAnalytic" RA
                                         JOIN public."Chapters" C ON C."Id" = RA."ChapterId"
                                         JOIN public."Volumes" V ON C."VolumeId" = V."Id"
                                WHERE RA."Timestamp" >= @From
                                  AND RA."Timestamp" <= @To
                                  AND V."SeriesId" = @SeriesId
                                GROUP BY TO_CHAR(RA."Timestamp", 'Day')) AS agg
                               ON TRIM(BOTH FROM d."DayName") = TRIM(BOTH FROM agg."DayName")
            GROUP BY d."DayName"
            ORDER BY ARRAY_POSITION(ARRAY ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'], d."DayName");
            """,
            new { range.From, range.To, SeriesId = seriesId }
        );
    }

    public async Task<(int Count, IEnumerable<ChapterRankingsDto> Data)> GetSeriesChapterViewRankingAsync(
        DateRange range,
        int seriesId,
        string query = "",
        bool isDescending = false,
        int pageNumber = 1,
        int pageSize = 5)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        string? sort = isDescending ? "DESC" : "ASC";

        int count = await connection.ExecuteScalarAsync<int>(
            """
            SELECT count(*)
            FROM "Chapters"
                     LEFT JOIN public."Volumes" V on V."Id" = "Chapters"."VolumeId"
            WHERE "SeriesId" = @SeriesId
            """,
            new { range.From, range.To, SeriesId = seriesId }
        );

        IEnumerable<ChapterRankingsDto>? result = await connection.QueryAsync<ChapterRankingsDto>($"""
                 SELECT C."Id",
                        C."ChapterNumber",
                        CV."Title" AS "Name",
                        C."VolumeId",
                        V."Name" AS "VolumeName",
                        sum(CASE WHEN RA."ViewCount" is null THEN 0 ELSE RA."ViewCount" END)                             AS "Data",
                        rank() over (ORDER BY sum(Case when RA."ViewCount" is null then 0 else RA."ViewCount" END) DESC) AS "Rank"
                 FROM public."Chapters" C
                          JOIN public."ChapterVersion" CV on C."CurrentVersionId" = CV."Id"
                          LEFT JOIN "ReadingAnalytic" RA on C."Id" = RA."ChapterId" AND RA."Timestamp" >= @From AND RA."Timestamp" <= @To
                          LEFT JOIN public."Volumes" V on C."VolumeId" = V."Id"
                 WHERE "SeriesId" = @SeriesId AND CV."Title" LIKE @ChapterTitle
                 GROUP BY C."Id", C."ChapterNumber", CV."Title", C."VolumeId", V."Name"
                 ORDER BY "Rank" {sort}
                 LIMIT @Limit OFFSET @Offset;
                 """,
            new
            {
                range.From,
                range.To,
                ChapterTitle = $"%{query}%",
                SeriesId = seriesId,
                Limit = pageSize,
                Offset = (pageNumber - 1) * pageSize
            });

        return new ValueTuple<int, IEnumerable<ChapterRankingsDto>>(count, result);
    }

    public async Task<int> GetSeriesCommentRankingAsync(DateRange range, int seriesId)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.ExecuteScalarAsync<int>(
            """
            SELECT "Rank"
            FROM (SELECT S."Id"                                     AS "SeriesId",
                         rank() over (ORDER BY count(C2."Id") DESC) AS "Rank"
                  FROM "Series" S
                           LEFT JOIN public."Volumes" V on S."Id" = V."SeriesId"
                           LEFT JOIN public."Chapters" C on V."Id" = C."VolumeId"
                           LEFT JOIN public."Comments" C2
                                     on C."Id" = C2."ChapterId" AND C2."Timestamp" >= @From AND C2."Timestamp" <= @To
                  GROUP BY S."Id"
                  ORDER BY "Rank") AS "Rankings"
            WHERE "Rankings"."SeriesId" = @SeriesId
            """,
            new { range.From, range.To, SeriesId = seriesId });
    }

    public async Task<int> GetSeriesCommentCountAsync(DateRange range, int seriesId)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.ExecuteScalarAsync<int>(
            """
            SELECT count(CMT."Id") AS "CommentCount"
            FROM "Comments" CMT
                     RIGHT JOIN public."Chapters" C ON C."Id" = CMT."ChapterId"
                     RIGHT JOIN public."Volumes" V ON C."VolumeId" = V."Id"
            WHERE CMT."Timestamp" >= @From
              AND CMT."Timestamp" <= @To
              AND "SeriesId" = @SeriesId;
            """,
            new { range.From, range.To, SeriesId = seriesId });
    }

    public async Task<IEnumerable<ChartDataDto>> GetSeriesCommentCountBreakdownInDayAsync(DateRange range, int seriesId)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.QueryAsync<ChartDataDto>(
            """
            WITH days AS (SELECT generate_series(@From::date, @To::date, '1 day'::interval)::date AS "Date")
            SELECT to_char(d."Date", 'DD-MM') AS "Time",
                   COALESCE(SUM(agg."CommentCount"), 0) AS "Data"
            FROM days d
                     LEFT JOIN (SELECT DATE(C."Timestamp") AS "Date",
                                       count(*) AS "CommentCount"
                                FROM "Comments" C
                                         JOIN public."Chapters" C2 on C2."Id" = C."ChapterId"
                                         JOIN public."Volumes" V on V."Id" = C2."VolumeId"
                                WHERE C."Timestamp" >= @From
                                  AND C."Timestamp" <= @To
                                  AND "SeriesId" = @SeriesId
                                GROUP BY "Date") AS agg ON d."Date" = agg."Date"
            GROUP BY d."Date"
            ORDER BY d."Date";
            """,
            new { range.From, range.To, SeriesId = seriesId });
    }

    public async Task<(int Count, IEnumerable<ChapterRankingsDto> Data)> GetSeriesChapterCommentRankingAsync(
        DateRange range,
        int seriesId,
        string query = "",
        bool isDescending = false,
        int pageNumber = 1,
        int pageSize = 5)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        int count = await connection.ExecuteScalarAsync<int>(
            """
            SELECT count(*)
            FROM "Chapters"
                     JOIN public."Volumes" V on V."Id" = "Chapters"."VolumeId"
            WHERE "SeriesId" = @SeriesId
            """,
            new { SeriesId = seriesId }
        );

        string? sort = isDescending ? "DESC" : "ASC";

        IEnumerable<ChapterRankingsDto>? result = await connection.QueryAsync<ChapterRankingsDto>(
            $"""
             SELECT C."Id",
                    C."ChapterNumber",
                    CV."Title"                                 AS "Name",
                    C."VolumeId",
                    V."Name"                                   AS "VolumeName",
                    count(C2."Id")                             AS "Data",
                    rank() over (ORDER BY count(C2."Id") DESC) AS "Rank"
             FROM "Chapters" C
                      JOIN public."ChapterVersion" CV on C."CurrentVersionId" = CV."Id"
                      LEFT JOIN public."Comments" C2 on C."Id" = C2."ChapterId" AND C2."Timestamp" >= @From AND C2."Timestamp" <= @To
                      LEFT JOIN public."Volumes" V on C."VolumeId" = V."Id"
             WHERE "SeriesId" = @SeriesId AND CV."Title" LIKE @ChapterTitle
             GROUP BY C."Id", C."ChapterNumber", CV."Title", C."VolumeId", V."Name"
             ORDER BY "Rank" {sort}
             LIMIT @Limit OFFSET @Offset;
             """,
            new
            {
                range.From,
                range.To,
                ChapterTitle = $"%{query}%",
                SeriesId = seriesId,
                Limit = pageSize,
                Offset = (pageNumber - 1) * pageSize
            }
        );

        return new ValueTuple<int, IEnumerable<ChapterRankingsDto>>(count, result);
    }

    public async Task<int> GetSeriesReviewRankingAsync(DateRange range, int seriesId)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.ExecuteScalarAsync<int>(
            """
            SELECT "Rank"
            FROM (SELECT S."Id"                                    AS "SeriesId",
                         rank() over (ORDER BY count(R."Id") DESC) AS "Rank"
                  FROM "Series" S
                           LEFT JOIN "Reviews" R ON S."Id" = R."SeriesId" AND R."Timestamp" >= @From AND R."Timestamp" <= @To
                  GROUP BY S."Id"
                  ORDER BY "Rank") AS "Rankings"
            WHERE "Rankings"."SeriesId" = @SeriesId;
            """,
            new { range.From, range.To, SeriesId = seriesId });
    }

    public async Task<(int ReviewCount, float Rating)> GetSeriesReviewCountAsync(DateRange range, int seriesId)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.QueryFirstAsync<(int ReviewCount, float Rating)>(
            """
            SELECT
            (SELECT count(*)
                         FROM "Reviews"
                         WHERE "Timestamp" >= @From
                           AND "Timestamp" <= @To
                           AND "SeriesId" = @SeriesId)                          AS "ReviewCount",
            (SELECT (CAST("RecommendedCount" AS DECIMAL) / "ReviewCount") * 100
             FROM (SELECT sum(CASE WHEN "IsRecommended" THEN 1 ELSE 0 END) AS "RecommendedCount",
                          count(*)                                         AS "ReviewCount"
                   FROM "Reviews"
                   WHERE "Timestamp" >= @From
                     AND "Timestamp" <= @To
                     AND "SeriesId" = @SeriesId) as ReviewAnalytic) AS "Rating"
            """,
            new { range.From, range.To, SeriesId = seriesId });
    }

    public async Task<IEnumerable<ReviewChartDataDto>> GetSeriesReviewCountBreakdownInDayAsync(
        DateRange range,
        int seriesId)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.QueryAsync<ReviewChartDataDto>(
            """
            WITH days AS (SELECT generate_series(@From::date, @To::date, '1 day'::interval)::date AS "Date")
            SELECT to_char(d."Date", 'DD-MM') AS "Time",
                   COALESCE(SUM(agg."RecommendedCount"), 0) AS "Positive",
                   COALESCE(SUM(agg."NotRecommendedCount"), 0) AS "Negative"
            FROM days d
                     LEFT JOIN (SELECT DATE("Timestamp")                                AS "Date",
                                       sum(CASE WHEN "IsRecommended" THEN 1 ELSE 0 END) AS "RecommendedCount",
                                       sum(CASE WHEN "IsRecommended" THEN 0 ELSE 1 END) AS "NotRecommendedCount"
                                FROM "Reviews" R
                                WHERE "Timestamp" >= @From
                                  AND "Timestamp" <= @To
                                  AND "SeriesId" = @SeriesId
                                GROUP BY "Date") AS agg ON d."Date" = agg."Date"
            GROUP BY d."Date"
            ORDER BY d."Date";
            """,
            new { range.From, range.To, SeriesId = seriesId });
    }

    public async Task<int> GetSeriesKanaRevenueAsync(DateRange range, int seriesId)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.ExecuteScalarAsync<int>(
            """
            SELECT sum(CASE WHEN "Amount" IS NULL THEN 0 ELSE "Amount" END) * -1 AS "TotalRevenue"
            FROM "KanaTransaction" KT
                     RIGHT JOIN public."Chapters" C on C."Id" = KT."ChapterId" AND KT."Timestamp" >= @From AND KT."Timestamp" <= @To
                     RIGHT JOIN public."Volumes" V on C."VolumeId" = V."Id"
            WHERE "SeriesId" = @SeriesId;
            """,
            new { range.From, range.To, SeriesId = seriesId });
    }

    public async Task<int> GetSeriesKanaRevenueRankingAsync(DateRange range, int seriesId)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.ExecuteScalarAsync<int>(
            """
            SELECT "Rank"
            FROM (SELECT "SeriesId"                                                                                AS "SeriesId",
                         rank() over (ORDER BY sum(CASE WHEN "Amount" IS NULL THEN 0 ELSE "Amount" END) * -1 DESC) AS "Rank"
                  FROM "Chapters" C
                           LEFT JOIN "KanaTransaction" KT
                                     on C."Id" = KT."ChapterId" AND KT."Timestamp" >= @From AND KT."Timestamp" <= @To
                           JOIN "Volumes" ON C."VolumeId" = "Volumes"."Id"
                  GROUP BY "SeriesId") AS "Rankings"
            WHERE "SeriesId" = @SeriesId;
            """,
            new { range.From, range.To, SeriesId = seriesId });
    }

    public async Task<IEnumerable<ChartDataDto>> GetSeriesKanaRevenueBreakdownInDayAsync(DateRange range, int seriesId)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.QueryAsync<ChartDataDto>(
            """
            WITH days AS (SELECT generate_series(@From::date, @To::date, '1 day'::interval)::date AS "Date")
            SELECT to_char(d."Date", 'DD-MM') AS "Time",
                   COALESCE(SUM(agg."KanaRevenue"), 0) AS "Data"
            FROM days d
                     LEFT JOIN (SELECT DATE(KT."Timestamp") AS "Date",
                                       sum("Amount") * -1   AS "KanaRevenue"
                                FROM "KanaTransaction" KT
                                         JOIN public."Chapters" C on C."Id" = KT."ChapterId"
                                         JOIN public."Volumes" V on V."Id" = C."VolumeId"
                                WHERE KT."Timestamp" >= @From
                                  AND KT."Timestamp" <= @To
                                  AND "SeriesId" = @SeriesId
                                GROUP BY "Date") AS agg ON d."Date" = agg."Date"
            GROUP BY d."Date"
            ORDER BY d."Date";
            """,
            new { range.From, range.To, SeriesId = seriesId });
    }

    public async Task<(int Count, IEnumerable<ChapterRankingsDto> Data)> GetSeriesChapterKanaRevenueRankingAsync(
        DateRange range,
        int seriesId,
        string query = "",
        bool isDescending = false,
        int pageNumber = 1,
        int pageSize = 5)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        int count = await connection.ExecuteScalarAsync<int>(
            """
            SELECT count(*)
            FROM "Chapters"
                     JOIN public."Volumes" V on V."Id" = "Chapters"."VolumeId"
            WHERE "SeriesId" = @SeriesId
            """,
            new { SeriesId = seriesId }
        );

        string? sort = isDescending ? "DESC" : "ASC";

        IEnumerable<ChapterRankingsDto>? result = await connection.QueryAsync<ChapterRankingsDto>(
            $"""
             SELECT C."Id",
                    C."ChapterNumber",
                    CV."Title"                                                                                      AS "Name",
                    C."VolumeId",
                    V."Name"                                                                                        AS "VolumeName",
                    sum(CASE WHEN KT."Amount" IS NULL THEN 0 ELSE KT."Amount" END) * -1                             AS "Data",
                    rank() over (ORDER BY sum(CASE WHEN KT."Amount" IS NULL THEN 0 ELSE KT."Amount" END) * -1 DESC) AS "Rank"
             FROM "Chapters" C
                      JOIN public."ChapterVersion" CV on C."CurrentVersionId" = CV."Id"
                      LEFT JOIN public."KanaTransaction" KT
                                on C."Id" = KT."ChapterId" AND KT."Timestamp" >= @From AND KT."Timestamp" <= @To
                      LEFT JOIN public."Volumes" V on C."VolumeId" = V."Id"
             WHERE "SeriesId" = @SeriesId
               AND CV."Title" LIKE @ChapterTitle
             GROUP BY C."Id", C."ChapterNumber", CV."Title", C."VolumeId", V."Name"
             ORDER BY "Rank" {sort}
             LIMIT @Limit OFFSET @Offset;
             """,
            new
            {
                range.From,
                range.To,
                ChapterTitle = $"%{query}%",
                SeriesId = seriesId,
                Limit = pageSize,
                Offset = (pageNumber - 1) * pageSize
            }
        );

        return new ValueTuple<int, IEnumerable<ChapterRankingsDto>>(count, result);
    }

    public async Task<int> GetSeriesDashiFanSubscriberCountAsync(DateRange range, int seriesId)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.ExecuteScalarAsync<int>(
            """
            SELECT count(DISTINCT SH."SubscriptionId") AS "DashiFanCount"
            FROM "SubscriptionHistory" SH
                     JOIN "Subscriptions" S ON S."Id" = SH."SubscriptionId"
                     JOIN "DashiFans" DF ON DF."Id" = S."DashiFanId"
            WHERE SH."Status" = @Status
              AND (SH."Timestamp" + INTERVAL '1 month') >= @From
              AND SH."Timestamp" <= @To
              AND "SeriesId" = @SeriesId
            """,
            new { range.From, range.To, SeriesId = seriesId, Status = SubscriptionStatus.Active });
    }

    public async Task<int> GetSeriesDashiFanRankingAsync(DateRange range, int seriesId)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.ExecuteScalarAsync<int>(
            """
            SELECT "Rank"
            FROM (SELECT S2."Id"                                                         AS "SeriesId",
                         rank() over (ORDER BY count(DISTINCT SH."SubscriptionId") DESC) AS "Rank"
                  FROM "SubscriptionHistory" SH
                           RIGHT JOIN "Subscriptions" S
                                      ON S."Id" = SH."SubscriptionId"
                                          AND (SH."Timestamp" + INTERVAL '1 month') >= @From
                                          AND SH."Status" = @Status
                                          AND SH."Timestamp" <= @To
                           RIGHT JOIN "DashiFans" DF ON DF."Id" = S."DashiFanId"
                           RIGHT JOIN public."Series" S2 on S2."Id" = DF."SeriesId"
                  GROUP BY S2."Id") AS "Rankings"
            WHERE "SeriesId" = @SeriesId;
            """,
            new { range.From, range.To, SeriesId = seriesId, Status = SubscriptionStatus.Active });
    }

    public async Task<IEnumerable<ChartDataDto>> GetSeriesDashiFanBreakdownInMonthAsync(DateRange range, int seriesId)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.QueryAsync<ChartDataDto>(
            """
            WITH months AS (SELECT generate_series(
                                           date_trunc('month', @From::date),
                                           date_trunc('month', @To::date),
                                           '1 month'::interval
                                   )::date AS "Month")
            SELECT to_char(m."Month", 'MM')                                 AS "Time",
                   COALESCE(SUM(agg."SubscriptionCount"), 0) AS "Data"
            FROM months m
                     LEFT JOIN (SELECT date_trunc('month', SH."Timestamp") AS "Month",
                                       count(DISTINCT SH."SubscriptionId") AS "SubscriptionCount"
                                FROM "SubscriptionHistory" SH
                                         JOIN "Subscriptions" S ON S."Id" = SH."SubscriptionId"
                                         JOIN "DashiFans" DF ON DF."Id" = S."DashiFanId"
                                WHERE SH."Status" = @Status
                                  AND (SH."Timestamp" + INTERVAL '1 month') >= @From
                                  AND SH."Timestamp" <= @To
                                  AND "SeriesId" = @SeriesId
                                GROUP BY "Month") AS agg ON m."Month" = agg."Month"
            GROUP BY m."Month"
            ORDER BY m."Month";
            """,
            new { range.From, range.To, SeriesId = seriesId, Status = SubscriptionStatus.Active });
    }

    public async Task<(int Count, IEnumerable<DashiFanRankingDto> Data)> GetSeriesDashiFanTierRankingAsync(
        DateRange range,
        int seriesId,
        string query = "",
        bool isDescending = false,
        int pageNumber = 1,
        int pageSize = 5)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        int count = await connection.ExecuteScalarAsync<int>(
            """
            SELECT count(*)
            FROM "DashiFans"
            WHERE "SeriesId" = @SeriesId;
            """,
            new { SeriesId = seriesId }
        );

        string? sort = isDescending ? "DESC" : "ASC";

        IEnumerable<DashiFanRankingDto>? result = await connection.QueryAsync<DashiFanRankingDto>(
            $"""
             SELECT DF."Id",
                    DF."Name"                                                       AS "Name",
                    count(DISTINCT SH."SubscriptionId")                             AS "Data",
                    rank() over (ORDER BY count(DISTINCT SH."SubscriptionId") DESC) AS "Rank"
             FROM "DashiFans" DF
                      LEFT JOIN "Subscriptions" S ON DF."Id" = S."DashiFanId"
                      LEFT JOIN "SubscriptionHistory" SH
                                ON S."Id" = SH."SubscriptionId"
                                    AND SH."Status" = @Status
                                    AND (SH."Timestamp" + INTERVAL '1 month') >= @From
                                    AND SH."Timestamp" <= @To
             WHERE DF."SeriesId" = @SeriesId AND DF."Name" LIKE @TierName
             GROUP BY DF."Id", DF."Name"
             ORDER BY "Rank" {sort}
             LIMIT @Limit OFFSET @Offset;
             """,
            new
            {
                range.From,
                range.To,
                TierName = $"%{query}%",
                SeriesId = seriesId,
                Limit = pageSize,
                Offset = (pageNumber - 1) * pageSize,
                Status = SubscriptionStatus.Active
            }
        );

        return new ValueTuple<int, IEnumerable<DashiFanRankingDto>>(count, result);
    }

    public async Task<IEnumerable<int>> GetTopSeries(string interval)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        string? deductTime = interval.ToLower() switch
        {
            "week" => "- INTERVAL '7 Days'",
            "month" => "- INTERVAL '30 Days'",
            "year" => "- INTERVAL '365 Days'",
            _ => "- INTERVAL '7 Days'"
        };

        return await connection.QueryAsync<int>(
            $"""
             SELECT
                 "TrendingSeries"."SeriesId"
             FROM
                 (SELECT S."Id" AS "SeriesId",
                         rank() over (ORDER BY sum(Case when RA."ViewCount" is null then 0 else RA."ViewCount" END) DESC) AS "Rank"
                  FROM "Series" S
                           left join "Volumes" V on S."Id" = V."SeriesId"
                           left join "Chapters" C on V."Id" = C."VolumeId"
                           left join "ReadingAnalytic" RA on C."Id" = RA."ChapterId" 
                                AND RA."Timestamp" >= now() {deductTime}
                                AND RA."Timestamp" <= now()
                  WHERE S."Status" NOT IN (0, 2, 4)
                  GROUP BY S."Id"
                  ORDER BY "Rank"
                  LIMIT 12) AS "TrendingSeries";
             """
        );
    }

    public async Task<IEnumerable<GenreDto>> GetTopGenres()
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.QueryAsync<GenreDto>(
            """
            SELECT G."Id",
                   G."Name",
                   sum(CASE WHEN C."ViewCount" is null THEN 0 ELSE C."ViewCount" END)                             AS "View",
                   rank() over (ORDER BY sum(CASE WHEN C."ViewCount" is null THEN 0 ELSE C."ViewCount" END) DESC) AS "Rank"
            FROM "Genres" G
                     LEFT JOIN "GenreSeries" GS ON G."Id" = GS."GenresId"
                     LEFT JOIN "Series" S ON GS."SeriesId" = S."Id"
                     LEFT JOIN "Volumes" V on S."Id" = V."SeriesId"
                     LEFT JOIN "Chapters" C on V."Id" = C."VolumeId"
            WHERE S."Status" NOT IN (0, 2, 4)
            GROUP BY G."Id",
                     G."Name"
            ORDER BY "Rank"
            LIMIT 5;
            """
        );
    }

    public async Task<IEnumerable<int>> GetTopGenresSeries(int genreId)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.QueryAsync<int>(
            """
            SELECT S."Id",
                   sum(CASE WHEN C."ViewCount" is null THEN 0 ELSE C."ViewCount" END)                             AS "View",
                   rank() over (ORDER BY sum(CASE WHEN C."ViewCount" is null THEN 0 ELSE C."ViewCount" END) DESC) AS "Rank"
            FROM "Genres" G
                     LEFT JOIN "GenreSeries" GS ON G."Id" = GS."GenresId"
                     LEFT JOIN "Series" S ON GS."SeriesId" = S."Id"
                     LEFT JOIN "Volumes" V on S."Id" = V."SeriesId"
                     LEFT JOIN "Chapters" C on V."Id" = C."VolumeId"
            WHERE S."Status" NOT IN (0, 2, 4) AND G."Id" = @GenreId
            GROUP BY S."Id"
            ORDER BY "Rank"
            LIMIT 12
            """,
            new { GenreId = genreId }
        );
    }
}
