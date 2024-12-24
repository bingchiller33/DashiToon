using System.Data;
using Dapper;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Reviews.Models;
using DashiToon.Api.Infrastructure.Data;

namespace DashiToon.Api.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public ReviewRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<ReviewDto?> FindReviewByUserAndSeries(int seriesId, string userId)
    {
        using IDbConnection? connection = await _dbConnectionFactory.CreateConnectionAsync();

        return await connection.QueryFirstOrDefaultAsync<ReviewDto?>(
            """
            SELECT "Reviews"."Id",
                   "UserId",
                   "UserName",
                   "Avatar",
                   "IsRecommended",
                   "Content",
                   "Likes",
                   "Dislikes",
                   "Timestamp",
                   "Created",
                   "LastModified"
            FROM "Reviews"
                     JOIN "Users" ON "Reviews"."UserId" = "Users"."Id"
            WHERE "UserId" = @UserId
              AND "SeriesId" = @SeriesId
            """,
            new { userId, seriesId });
    }

    public async Task<(int Count, IEnumerable<ReviewDto> Result)> FindReviews(
        int seriesId,
        int pageNumber,
        int pageSize,
        string sortBy)
    {
        using IDbConnection? connection = await _dbConnectionFactory.CreateConnectionAsync();

        int count = await connection.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(*)
            FROM "Reviews"
                     JOIN "Users" ON "Reviews"."UserId" = "Users"."Id"
            WHERE "SeriesId" = @SeriesId
            """,
            new { SeriesId = seriesId, Limit = pageSize, Offset = (pageNumber - 1) * pageSize });

        string? sql = sortBy switch
        {
            "Best" => """
                      SELECT "Reviews"."Id",
                             "UserId",
                             "UserName",
                             "Avatar",
                             "IsRecommended",
                             "Content",
                             "Likes",
                             "Dislikes",
                             "Timestamp",
                             "Created",
                             "LastModified"
                      FROM "Reviews"
                               JOIN "Users" ON "Reviews"."UserId" = "Users"."Id"
                      WHERE "SeriesId" = @SeriesId
                      ORDER BY "IsRecommended" DESC, "Likes" - "Dislikes" DESC 
                      LIMIT @Limit OFFSET @Offset
                      """,
            "Worst" => """
                       SELECT "Reviews"."Id",
                              "UserId",
                              "UserName",
                              "Avatar",
                              "IsRecommended",
                              "Content",
                              "Likes",
                              "Dislikes",
                              "Timestamp",
                              "Created",
                              "LastModified"
                       FROM "Reviews"
                                JOIN "Users" ON "Reviews"."UserId" = "Users"."Id"
                       WHERE "SeriesId" = @SeriesId
                       ORDER BY "IsRecommended", "Likes" - "Dislikes" DESC 
                       LIMIT @Limit OFFSET @Offset
                       """,
            "Newest" => """
                        SELECT "Reviews"."Id",
                               "UserId",
                               "UserName",
                               "Avatar",
                               "IsRecommended",
                               "Content",
                               "Likes",
                               "Dislikes",
                               "Timestamp",
                               "Created",
                               "LastModified"
                        FROM "Reviews"
                                 JOIN "Users" ON "Reviews"."UserId" = "Users"."Id"
                        WHERE "SeriesId" = @SeriesId
                        ORDER BY "LastModified" DESC, "Likes" - "Dislikes" DESC 
                        LIMIT @Limit OFFSET @Offset
                        """,
            "Oldest" => """
                        SELECT "Reviews"."Id",
                               "UserId",
                               "UserName",
                               "Avatar",
                               "IsRecommended",
                               "Content",
                               "Likes",
                               "Dislikes",
                               "Timestamp",
                               "Created",
                               "LastModified"
                        FROM "Reviews"
                                 JOIN "Users" ON "Reviews"."UserId" = "Users"."Id"
                        WHERE "SeriesId" = @SeriesId
                        ORDER BY "LastModified", "Likes" - "Dislikes" DESC 
                        LIMIT @Limit OFFSET @Offset
                        """,
            _ => """
                 SELECT "Reviews"."Id",
                        "UserId",
                        "UserName",
                        "Avatar",
                        "IsRecommended",
                        "Content",
                        "Likes",
                        "Dislikes",
                        "Timestamp",
                        "Created",
                        "LastModified"
                 FROM "Reviews"
                          JOIN "Users" ON "Reviews"."UserId" = "Users"."Id"
                 WHERE "SeriesId" = @SeriesId
                 ORDER BY "Likes" - "Dislikes" DESC 
                 LIMIT @Limit OFFSET @Offset
                 """
        };

        IEnumerable<ReviewDto>? result = await connection.QueryAsync<ReviewDto>(
            sql,
            new { SeriesId = seriesId, Limit = pageSize, Offset = (pageNumber - 1) * pageSize });

        return new ValueTuple<int, IEnumerable<ReviewDto>>(count, result);
    }
}
