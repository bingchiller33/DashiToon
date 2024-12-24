using System.Data;
using Dapper;
using DashiToon.Api.Application.Comments.Models;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Infrastructure.Data;

namespace DashiToon.Api.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CommentRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<(int Count, IEnumerable<CommentDto> Result)> FindComments(
        int chapterId,
        int pageNumber,
        int pageSize,
        string sortBy)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        int count = await connection.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(*)
            FROM "Comments"
                     JOIN "Users" ON "Comments"."UserId" = "Users"."Id"
            WHERE "ChapterId" = @ChapterId
              AND "ParentCommentId" IS NULL
            """,
            new { ChapterId = chapterId });

        string? sql = sortBy switch
        {
            "Newest" => """
                        SELECT "Comments"."Id",
                               "UserId",
                               "UserName",
                               "Avatar",
                               "Content",
                               "Likes",
                               "Dislikes",
                               (WITH RECURSIVE CommentReplies AS (SELECT c1."Id",
                                                                         1 as depth
                                                                  FROM "Comments" c1
                                                                  WHERE c1."ParentCommentId" = "Comments"."Id"
                        
                                                                  UNION ALL
                        
                                                                  SELECT c2."Id",
                                                                         cr.depth + 1
                                                                  FROM "Comments" c2
                                                                           INNER JOIN CommentReplies cr ON c2."ParentCommentId" = cr."Id")
                                SELECT count(*)
                                FROM CommentReplies) AS "RepliesCount",
                               "Timestamp",
                               "Created",
                               "LastModified"
                        FROM "Comments"
                                 JOIN "Users" ON "Comments"."UserId" = "Users"."Id"
                        WHERE "ChapterId" = @ChapterId
                          AND "ParentCommentId" IS NULL
                        ORDER BY "LastModified" DESC, "Likes" - "Dislikes" DESC
                        LIMIT @Limit OFFSET @Offset
                        """,
            "Oldest" => """
                        SELECT "Comments"."Id",
                               "UserId",
                               "UserName",
                               "Avatar",
                               "Content",
                               "Likes",
                               "Dislikes",
                               (WITH RECURSIVE CommentReplies AS (SELECT c1."Id",
                                                                         1 as depth
                                                                  FROM "Comments" c1
                                                                  WHERE c1."ParentCommentId" = "Comments"."Id"
                        
                                                                  UNION ALL
                        
                                                                  SELECT c2."Id",
                                                                         cr.depth + 1
                                                                  FROM "Comments" c2
                                                                           INNER JOIN CommentReplies cr ON c2."ParentCommentId" = cr."Id")
                                SELECT count(*)
                                FROM CommentReplies) AS "RepliesCount",
                               "Timestamp",
                               "Created",
                               "LastModified"
                        FROM "Comments"
                                 JOIN "Users" ON "Comments"."UserId" = "Users"."Id"
                        WHERE "ChapterId" = @ChapterId
                          AND "ParentCommentId" IS NULL
                        ORDER BY "LastModified", "Likes" - "Dislikes" DESC
                        LIMIT @Limit OFFSET @Offset
                        """,
            _ => """
                 SELECT "Comments"."Id",
                        "UserId",
                        "UserName",
                        "Avatar",
                        "Content",
                        "Likes",
                        "Dislikes",
                        (WITH RECURSIVE CommentReplies AS (SELECT c1."Id",
                                                                  1 as depth
                                                           FROM "Comments" c1
                                                           WHERE c1."ParentCommentId" = "Comments"."Id"
                 
                                                           UNION ALL
                 
                                                           SELECT c2."Id",
                                                                  cr.depth + 1
                                                           FROM "Comments" c2
                                                                    INNER JOIN CommentReplies cr ON c2."ParentCommentId" = cr."Id")
                         SELECT count(*)
                         FROM CommentReplies) AS "RepliesCount",
                        "Timestamp",
                        "Created",
                        "LastModified"
                 FROM "Comments"
                          JOIN "Users" ON "Comments"."UserId" = "Users"."Id"
                 WHERE "ChapterId" = @ChapterId
                   AND "ParentCommentId" IS NULL
                 ORDER BY "Likes" - "Dislikes" DESC
                 LIMIT @Limit OFFSET @Offset
                 """
        };

        IEnumerable<CommentDto>? result = await connection.QueryAsync<CommentDto>(
            sql,
            new { ChapterId = chapterId, Limit = pageSize, Offset = (pageNumber - 1) * pageSize });

        return new ValueTuple<int, IEnumerable<CommentDto>>(count, result);
    }

    public async Task<IEnumerable<ReplyDto>> FindCommentReplies(int chapterId, Guid commentId)
    {
        using IDbConnection? connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.QueryAsync<ReplyDto>(
            """
            WITH RECURSIVE CommentHierarchy AS (
                SELECT "Id",
                       "ParentCommentId",
                       "UserId",
                       "Content",
                       "Likes",
                       "Dislikes",
                       "Timestamp",
                       "Created",
                       "LastModified",
                       1 AS "Depth"
                FROM "Comments"
                WHERE "Id" = @CommentId 
            
                UNION ALL
            
                SELECT c."Id",
                       c."ParentCommentId",
                       c."UserId",
                       c."Content",
                       c."Likes",
                       c."Dislikes",
                       c."Timestamp",
                       c."Created",
                       c."LastModified",
                       ch."Depth" + 1 AS "Depth"
                FROM "Comments" c
                         JOIN CommentHierarchy ch ON c."ParentCommentId" = ch."Id")
            SELECT ch."Id",
                   ch."ParentCommentId",
                   ch."UserId",
                   "UserName",
                   "Avatar",
                   ch."Content",
                   ch."Likes",
                   ch."Dislikes",
                   ch."Timestamp",
                   ch."Created",
                   ch."LastModified",
                   ch."Depth"
            FROM CommentHierarchy ch
                     JOIN "Users" ON ch."UserId" = "Users"."Id"
            ORDER BY ch."Depth", ch."Likes" - ch."Dislikes" DESC;
            """,
            new { CommentId = commentId }
        );
    }
}
