using System.Data;
using Dapper;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Moderation.Model;
using DashiToon.Api.Application.Moderation.Queries.GetChapterReports;
using DashiToon.Api.Application.Moderation.Queries.GetCommentReports;
using DashiToon.Api.Application.Moderation.Queries.GetReviewReports;
using DashiToon.Api.Application.Moderation.Queries.GetSeriesReports;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Infrastructure.Data;

namespace DashiToon.Api.Infrastructure.Repositories;

public sealed class ReportRepository : IReportRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ReportRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<(int Count, IEnumerable<CommentReportDto> Result)> FindCommentReports(
        int pageNumber,
        int pageSize)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        int count = await connection.ExecuteScalarAsync<int>(
            """
            SELECT count(distinct "Comments"."Id")
            FROM "Comments"
                     JOIN public."Reports" R ON "Comments"."Id" = R."CommentId"
            WHERE R."ReportStatus" = @ReportStatus     
            """,
            new { ReportStatus = ReportStatus.Pending }
        );

        IEnumerable<CommentReportDto>? result = await connection.QueryAsync<CommentReportDto>(
            """
            SELECT "Comments"."Id"      AS "CommentId",
                   "Comments"."Content" AS "CommentContent",
                   U2."UserName"        AS "CommentUser",
                   "Chapters"."ChapterNumber",
                   "Volumes"."VolumeNumber",
                   "Series"."Title"     AS "SeriesTitle",
                   jsonb_agg(json_build_object(
                           'ReportedUser', CASE U1."UserName" IS NULL
                                                     WHEN True THEN 'Hệ thống'
                                                     ELSE U1."UserName" END,
                           'Reason', R."Reason",
                           'ReportedAt', R."ReportedAt",
                           'AnalysisFlagged', R."Analysis_Flagged",
                           'AnalysisFlaggedCategoriesString', R."Analysis_FlaggedCategories"
                             ))         AS "Reports"
            FROM "Comments"
                     JOIN public."Reports" R ON "Comments"."Id" = R."CommentId"
                     LEFT JOIN "Users" U1 ON R."Reported" = U1."Id"
                     LEFT JOIN "Users" U2 ON "Comments"."UserId" = U2."Id"
                     LEFT JOIN "Chapters" ON "Comments"."ChapterId" = "Chapters"."Id"
                     LEFT JOIN "Volumes" ON "Chapters"."VolumeId" = "Volumes"."Id"
                     LEFT JOIN "Series" ON "Volumes"."SeriesId" = "Series"."Id"
            WHERE R."ReportStatus" = @ReportStatus
            GROUP BY "Comments"."Id",
                     "Comments"."Content",
                     U2."UserName",
                     "Chapters"."ChapterNumber",
                     "Volumes"."VolumeNumber",
                     "Series"."Title"
            LIMIT @Limit OFFSET @Offset;
            """,
            new { ReportStatus = ReportStatus.Pending, Limit = pageSize, Offset = (pageNumber - 1) * pageSize }
        );

        return new ValueTuple<int, IEnumerable<CommentReportDto>>(count, result);
    }

    public async Task<(int Count, IEnumerable<ReviewReportDto> Result)> FindReviewReports(int pageNumber, int pageSize)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        int count = await connection.ExecuteScalarAsync<int>(
            """
            SELECT count(distinct "Reviews"."Id")
            FROM "Reviews"
                     JOIN public."Reports" R ON "Reviews"."Id" = R."ReviewId"
            WHERE R."ReportStatus" = @ReportStatus        
            """,
            new { ReportStatus = ReportStatus.Pending }
        );

        IEnumerable<ReviewReportDto>? result = await connection.QueryAsync<ReviewReportDto>(
            """
            SELECT "Reviews"."Id"      AS "ReviewId",
                   "Reviews"."Content" AS "ReviewContent",
                   U2."UserName"       AS "ReviewUser",
                   "Series"."Id"       AS "SeriesId",
                   "Series"."Title"    AS "SeriesTitle",
                   jsonb_agg(json_build_object(
                           'ReportedUser', CASE U1."UserName" IS NULL
                                                     WHEN True THEN 'Hệ thống'
                                                     ELSE U1."UserName" END,
                           'Reason', R."Reason",
                           'ReportedAt', R."ReportedAt",
                           'AnalysisFlagged', R."Analysis_Flagged",
                           'AnalysisFlaggedCategoriesString', R."Analysis_FlaggedCategories"
                             ))        AS "Reports"
            FROM "Reviews"
                     JOIN public."Reports" R ON "Reviews"."Id" = R."ReviewId"
                     LEFT JOIN "Users" U1 ON R."Reported" = U1."Id"
                     LEFT JOIN "Users" U2 ON "Reviews"."UserId" = U2."Id"
                     LEFT JOIN "Series" ON "Reviews"."SeriesId" = "Series"."Id"
            WHERE R."ReportStatus" = @ReportStatus
            GROUP BY "Reviews"."Id",
                     "Reviews"."Content",
                     U2."UserName",
                     "Series"."Id",
                     "Series"."Title"
            LIMIT @Limit OFFSET @Offset;
            """,
            new { ReportStatus = ReportStatus.Pending, Limit = pageSize, Offset = (pageNumber - 1) * pageSize }
        );

        return new ValueTuple<int, IEnumerable<ReviewReportDto>>(count, result);
    }

    public async Task<(int Count, IEnumerable<ChapterReportDto> Result)> FindChapterReports(int pageNumber,
        int pageSize)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        int count = await connection.ExecuteScalarAsync<int>(
            """
            SELECT count(distinct "Chapters"."Id")
            FROM "Chapters"
                     JOIN public."Reports" R ON "Chapters"."Id" = R."ChapterId"
            WHERE R."ReportStatus" = @ReportStatus;       
            """,
            new { ReportStatus = ReportStatus.Pending }
        );

        IEnumerable<ChapterReportDto>? result = await connection.QueryAsync<ChapterReportDto>(
            """
            SELECT "Chapters"."Id"  AS "ChapterId",
                   "ChapterNumber",
                   "Volumes"."Id"   AS "VolumeId",
                   "Volumes"."VolumeNumber",
                   "Series"."Id"    AS "SeriesId",
                   "Series"."Title" AS "SeriesTitle",
                   "Series"."Type"  AS "SeriesType",
                   U2."UserName"    AS "SeriesAuthor",
                   jsonb_agg(json_build_object(
                           'ReportedUser', CASE U1."UserName" IS NULL
                                                     WHEN True THEN 'Hệ thống'
                                                     ELSE U1."UserName" END,
                           'Reason', R."Reason",
                           'ReportedAt', R."ReportedAt",
                           'AnalysisFlagged', R."Analysis_Flagged",
                           'AnalysisFlaggedCategoriesString', R."Analysis_FlaggedCategories"
                             ))     AS "Reports"
            FROM "Chapters"
                     JOIN public."Reports" R ON "Chapters"."Id" = R."ChapterId"
                     LEFT JOIN "Users" U1 ON R."Reported" = U1."Id"
                     LEFT JOIN "Volumes" ON "Chapters"."VolumeId" = "Volumes"."Id"
                     LEFT JOIN "Series" ON "Volumes"."SeriesId" = "Series"."Id"
                     LEFT JOIN "Users" U2 ON "Series"."CreatedBy" = U2."Id"
            WHERE R."ReportStatus" = @ReportStatus
            GROUP BY "Chapters"."Id",
                     "ChapterNumber",
                     "Volumes"."Id",
                     "Volumes"."VolumeNumber",
                     "Series"."Id",
                     "Series"."Title",
                     "Series"."Type",
                     U2."UserName"
            LIMIT @Limit OFFSET @Offset;
            """,
            new { ReportStatus = ReportStatus.Pending, Limit = pageSize, Offset = (pageNumber - 1) * pageSize }
        );

        return new ValueTuple<int, IEnumerable<ChapterReportDto>>(count, result);
    }

    public async Task<(int Count, IEnumerable<SeriesReportDto> Result)> FindSeriesReports(int pageNumber, int pageSize)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        int count = await connection.ExecuteScalarAsync<int>(
            """
            SELECT count(distinct "Series"."Id")
            FROM "Series"
                     JOIN public."Reports" R ON "Series"."Id" = R."SeriesId"
            WHERE R."ReportStatus" = @ReportStatus; 
            """,
            new { ReportStatus = ReportStatus.Pending }
        );

        IEnumerable<SeriesReportDto>? result = await connection.QueryAsync<SeriesReportDto>(
            """
            SELECT "Series"."Id" AS "SeriesId",
                   "Title"       AS "SeriesTitle",
                   "Thumbnail"   AS "SeriesThumbnail",
                   "Synopsis"    AS "SeriesSynopsis",
                   U2."UserName" AS "SeriesAuthor",
                   jsonb_agg(json_build_object(
                           'ReportedUser', CASE U1."UserName" IS NULL
                                                     WHEN True THEN 'Hệ thống'
                                                     ELSE U1."UserName" END,
                           'Reason', R."Reason",
                           'ReportedAt', R."ReportedAt",
                           'AnalysisFlagged', R."Analysis_Flagged",
                           'AnalysisFlaggedCategoriesString', R."Analysis_FlaggedCategories"
                             ))  AS "Reports"
            FROM "Series"
                     JOIN public."Reports" R ON "Series"."Id" = R."SeriesId"
                     LEFT JOIN "Users" U1 ON R."Reported" = U1."Id"
                     LEFT JOIN "Users" U2 ON "Series"."CreatedBy" = U2."Id"
            WHERE R."ReportStatus" = @ReportStatus
            GROUP BY "Series"."Id",
                     "Title",
                     "Thumbnail",
                     "Synopsis",
                     U2."UserName"
            LIMIT @Limit OFFSET @Offset;
            """,
            new { ReportStatus = ReportStatus.Pending, Limit = pageSize, Offset = (pageNumber - 1) * pageSize }
        );

        return new ValueTuple<int, IEnumerable<SeriesReportDto>>(count, result);
    }
}
