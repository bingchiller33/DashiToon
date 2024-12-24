using DashiToon.Api.Application.Comments.Commands.RateComment;
using DashiToon.Api.Application.Comments.Commands.ReplyComment;
using DashiToon.Api.Application.Comments.Commands.UpdateComment;
using DashiToon.Api.Application.Comments.Commands.WriteComment;
using DashiToon.Api.Application.Comments.Models;
using DashiToon.Api.Application.Comments.Queries.GetCommentReplies;
using DashiToon.Api.Application.Comments.Queries.GetComments;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.ReadContent.Commands.IncrementViewCount;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DashiToon.Api.Web.Endpoints;

public class Chapters : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapPost(WriteComment, "{id}/comments")
            .MapPut(UpdateComment, "{id}/comments/{commentId}")
            .MapPost(RateComment, "{id}/comments/{commentId}/rate")
            .MapPost(ReplyComment, "{id}/comments/{commentId}/replies");

        app
            .MapGroup(this)
            .MapGet(GetChapterComments, "{id}/comments")
            .MapGet(GetChapterCommentReplies, "{id}/comments/{commentId}/replies");

        app
            .MapGroup(this)
            .MapPost(IncrementViewCount, "{id}/analytics/view");
    }

    public async Task<PaginatedList<CommentVm>> GetChapterComments(
        ISender sender,
        int id,
        string sortBy = "Top",
        int pageNumber = 1,
        int pageSize = 10)
    {
        return await sender.Send(new GetChapterCommentsQuery(id, pageNumber, pageSize, sortBy));
    }

    public async Task<List<ReplyVm>> GetChapterCommentReplies(ISender sender, int id, Guid commentId)
    {
        return await sender.Send(new GetCommentRepliesQuery(id, commentId));
    }

    public async Task<Results<BadRequest, Ok<CommentVm>>> WriteComment(
        ISender sender,
        int id,
        WriteCommentCommand command)
    {
        if (id != command.ChapterId)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(await sender.Send(command));
    }

    public async Task<Results<BadRequest, Ok<CommentVm>>> UpdateComment(
        ISender sender,
        int id,
        Guid commentId,
        UpdateCommentCommand command)
    {
        if (id != command.ChapterId || commentId != command.CommentId)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(await sender.Send(command));
    }

    public async Task<Results<BadRequest, Ok<CommentVm>>> RateComment(
        ISender sender,
        int id,
        Guid commentId,
        RateCommentCommand command)
    {
        if (id != command.ChapterId || commentId != command.CommentId)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(await sender.Send(command));
    }

    public async Task<Results<BadRequest, Ok<CommentVm>>> ReplyComment(
        ISender sender,
        int id,
        Guid commentId,
        ReplyCommentCommand command)
    {
        if (id != command.ChapterId || commentId != command.CommentId)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(await sender.Send(command));
    }

    public async Task<IResult> IncrementViewCount(
        HttpContext context,
        ISender sender,
        int id)
    {
        string? remoteIp = context.Connection.RemoteIpAddress?.ToString();

        await sender.Send(new IncrementViewCountCommand(remoteIp, id));

        return Results.Ok();
    }
}
