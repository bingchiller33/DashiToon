using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Moderation.Commands.DimissReports;
using DashiToon.Api.Application.Moderation.Commands.ReportChapter;
using DashiToon.Api.Application.Moderation.Commands.ReportComment;
using DashiToon.Api.Application.Moderation.Commands.ReportReview;
using DashiToon.Api.Application.Moderation.Commands.ReportSeries;
using DashiToon.Api.Application.Moderation.Commands.ResolveChapterReport;
using DashiToon.Api.Application.Moderation.Commands.ResolveCommentReport;
using DashiToon.Api.Application.Moderation.Commands.ResolveReviewReport;
using DashiToon.Api.Application.Moderation.Commands.ResolveSeriesReport;
using DashiToon.Api.Application.Moderation.Model;
using DashiToon.Api.Application.Moderation.Queries.GetChapterReports;
using DashiToon.Api.Application.Moderation.Queries.GetCommentReports;
using DashiToon.Api.Application.Moderation.Queries.GetReviewReports;
using DashiToon.Api.Application.Moderation.Queries.GetSeriesReports;
using DashiToon.Api.Application.Moderation.Queries.IsAuthorAllowedToPublish;
using DashiToon.Api.Application.Moderation.Queries.IsUserAllowedToReviewOrComment;

namespace DashiToon.Api.Web.Endpoints;

public class Moderation : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapPost(ReportComment, "comments/reports")
            .MapPost(ReportReview, "reviews/reports")
            .MapPost(ReportChapter, "chapters/reports")
            .MapPost(ReportSeries, "series/reports")
            .MapPost(DismissReports, "dismiss-report")
            .MapPost(ResolveChapterReport, "chapters/{chapterId}/reports/resolve")
            .MapPost(ResolveSeriesReport, "series/{seriesId}/reports/resolve")
            .MapPost(ResolveCommentReport, "comment/{commentId}/reports/resolve")
            .MapPost(ResolveReviewReport, "review/{reviewId}/reports/resolve");

        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetReviewReports, "reviews/reports")
            .MapGet(GetCommentReports, "comments/reports")
            .MapGet(GetChapterReports, "chapters/reports")
            .MapGet(GetSeriesReports, "series/reports");

        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(IsUserAllowedToReviewOrComment, "is-allowed-review-or-comment")
            .MapGet(IsAuthorAllowedToPublishQuery, "is-allowed-to-publish");
    }

    public async Task ReportComment(ISender sender, ReportCommentCommand command)
    {
        await sender.Send(command);
    }

    public async Task ReportReview(ISender sender, ReportReviewCommand command)
    {
        await sender.Send(command);
    }

    public async Task ReportChapter(ISender sender, ReportChapterCommand command)
    {
        await sender.Send(command);
    }

    public async Task ReportSeries(ISender sender, ReportSeriesCommand command)
    {
        await sender.Send(command);
    }

    public async Task DismissReports(ISender sender, DismissReportsCommand command)
    {
        await sender.Send(command);
    }

    public async Task<IResult> ResolveChapterReport(ISender sender, int chapterId, ResolveChapterReportCommand command)
    {
        if (chapterId != command.ChapterId)
        {
            return Results.BadRequest();
        }

        await sender.Send(command);

        return Results.NoContent();
    }

    public async Task<IResult> ResolveSeriesReport(ISender sender, int seriesId, ResolveSeriesReportCommand command)
    {
        if (seriesId != command.SeriesId)
        {
            return Results.BadRequest();
        }

        await sender.Send(command);

        return Results.NoContent();
    }

    public async Task<IResult> ResolveCommentReport(ISender sender, Guid commentId, ResolveCommentReportCommand command)
    {
        if (commentId != command.CommentId)
        {
            return Results.BadRequest();
        }

        await sender.Send(command);

        return Results.NoContent();
    }

    public async Task<IResult> ResolveReviewReport(ISender sender, Guid reviewId, ResolveReviewReportCommand command)
    {
        if (reviewId != command.ReviewId)
        {
            return Results.BadRequest();
        }

        await sender.Send(command);

        return Results.NoContent();
    }

    public Task<PaginatedList<ReviewReportVm>> GetReviewReports(ISender sender, int pageNumber = 1, int pageSize = 10)
    {
        return sender.Send(new GetReviewReportsQuery(pageNumber, pageSize));
    }

    public Task<PaginatedList<CommentReportVm>> GetCommentReports(ISender sender, int pageNumber = 1, int pageSize = 10)
    {
        return sender.Send(new GetCommentReportsQuery(pageNumber, pageSize));
    }

    public Task<PaginatedList<ChapterReportVm>> GetChapterReports(ISender sender, int pageNumber = 1, int pageSize = 10)
    {
        return sender.Send(new GetChapterReportsQuery(pageNumber, pageSize));
    }

    public Task<PaginatedList<SeriesReportVm>> GetSeriesReports(ISender sender, int pageNumber = 1, int pageSize = 10)
    {
        return sender.Send(new GetSeriesReportsQuery(pageNumber, pageSize));
    }

    public Task<AllowVm> IsUserAllowedToReviewOrComment(ISender sender)
    {
        return sender.Send(new IsUserAllowedToReviewOrCommentQuery());
    }

    public Task<AllowVm> IsAuthorAllowedToPublishQuery(ISender sender)
    {
        return sender.Send(new IsAuthorAllowedToPublishQuery());
    }
}
