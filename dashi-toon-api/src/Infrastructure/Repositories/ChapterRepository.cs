using System.Data;
using Dapper;
using DashiToon.Api.Application.AuthorStudio.Chapters.Models;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.ReadContent.Queries.Models;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using DashiToon.Api.Infrastructure.Data;

namespace DashiToon.Api.Infrastructure.Repositories;

public class ChapterRepository : IChapterRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ChapterRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Chapter?> FindChapterById(int chapterId)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.QueryFirstOrDefaultAsync<Chapter>(
            """
            SELECT *
            FROM "Chapters"
            WHERE "Id" = @Id;
            """,
            new { Id = chapterId });
    }

    public async Task<(int Count, IEnumerable<ChapterInfo> Chapters)> FindChapters(
        string? title,
        ChapterStatus? status,
        int volumeId,
        int pageNumber = 1,
        int pageSize = 10)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        int count = await connection.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(*)
            FROM "Chapters"
                     join "ChapterVersion" on "CurrentVersionId" = "ChapterVersion"."Id"
            WHERE (@Title = '' OR "Title" LIKE CONCAT('%', @Title, '%'))
              AND (@Status = -1 OR "Status" = @Status)
              AND "VolumeId" = @VolumeId
            """,
            new { Title = title ?? string.Empty, Status = status is null ? -1 : (int)status, VolumeId = volumeId });

        IEnumerable<ChapterInfo> chapters = await connection.QueryAsync<ChapterInfo>(
            """
            SELECT "ChapterId", "ChapterNumber", "Title", "Thumbnail", "Status", "KanaPrice", "PublishedDate"
            FROM "Chapters"
                     join "ChapterVersion" on "CurrentVersionId" = "ChapterVersion"."Id"
            WHERE (@Title = '' OR "Title" LIKE CONCAT('%', @Title, '%'))
              AND (@Status = -1 OR "Status" = @Status)
              AND "VolumeId" = @VolumeId
            ORDER BY "ChapterNumber" DESC
            LIMIT @Limit OFFSET @Offset;
            """,
            new
            {
                Title = title ?? string.Empty,
                Status = status is null ? -1 : (int)status,
                VolumeId = volumeId,
                Limit = pageSize,
                Offset = (pageNumber - 1) * pageSize
            });

        return (count, chapters);
    }

    public async Task<IEnumerable<ChapterInfo>> FindPublishedChaptersByVolumeId(int volumeId)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        IEnumerable<ChapterInfo> chapters = await connection.QueryAsync<ChapterInfo>(
            """
            SELECT "ChapterId", "ChapterNumber", "Title", "Thumbnail", "Status", "KanaPrice", "PublishedDate"
            FROM "Chapters"
                     JOIN "ChapterVersion" on "PublishedVersionId" = "ChapterVersion"."Id"
            WHERE "VolumeId" = @VolumeId
            ORDER BY "ChapterNumber" DESC;
            """,
            new { VolumeId = volumeId });

        return chapters;
    }

    public async Task<ChapterContent?> FindPublishedChapterById(int volumeId, int chapterId)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        ChapterContent? chapter = await connection.QueryFirstOrDefaultAsync<ChapterContent>(
            """
            SELECT "ChapterId", "ChapterNumber", "Title", "Thumbnail", "Content", "KanaPrice", "Status", "PublishedDate"
            FROM "Chapters"
                     join "ChapterVersion" on "PublishedVersionId" = "ChapterVersion"."Id"
            WHERE "VolumeId" = @VolumeId
               AND "ChapterId" = @ChapterId
            ORDER BY "ChapterNumber" DESC;
            """,
            new { VolumeId = volumeId, ChapterId = chapterId });

        return chapter;
    }


    public async Task<(int Count, IEnumerable<ChapterVersion> Versions)> FindChapterVersions(
        int id,
        string versionName,
        DateTimeOffset? from,
        DateTimeOffset? to,
        bool includeAutoSave = false,
        int pageNumber = 1,
        int pageSize = 10)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        int count = await connection.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(*)
            FROM "ChapterVersion"
            WHERE "ChapterId" = @Id
              AND (@VersionName = '' OR "VersionName" = @VersionName)
              AND (@IncludeAutoSave = true OR "IsAutoSave" = @IncludeAutoSave)
              AND (@From IS NULL OR "Timestamp" >= @From)
              AND (@To IS NULL OR "Timestamp" <= @To)
            """,
            new
            {
                id,
                versionName,
                includeAutoSave,
                from,
                to
            });

        IEnumerable<ChapterVersion> versions = await connection.QueryAsync<ChapterVersion>(
            """
            SELECT *
            FROM "ChapterVersion"
            WHERE "ChapterId" = @Id
              AND (@VersionName = '' OR "VersionName" = @VersionName)
              AND (@IncludeAutoSave = true OR "IsAutoSave" = @IncludeAutoSave)
              AND (@From IS NULL OR "Timestamp" >= @From)
              AND (@To IS NULL OR "Timestamp" <= @To)
            ORDER BY "Timestamp" DESC
            LIMIT @Limit offset @Offset;
            """,
            new
            {
                id,
                versionName,
                includeAutoSave,
                from,
                to,
                Limit = pageSize,
                Offset = (pageNumber - 1) * pageSize
            });

        return (count, versions);
    }

    public async Task<IEnumerable<int>> FindUserUnlockedChapterIds(string? userId)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        IEnumerable<int> chapters = await connection.QueryAsync<int>(
            """
            SELECT "UnlockedChaptersId" FROM "ApplicationUserChapter" WHERE "ApplicationUserId" = @UserId;
            """,
            new { UserId = userId });

        return chapters;
    }
}
