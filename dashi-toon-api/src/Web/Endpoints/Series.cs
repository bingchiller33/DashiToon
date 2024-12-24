using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Reviews.Commands.RateReviewCommand;
using DashiToon.Api.Application.Reviews.Commands.UpdateSeriesReviewCommand;
using DashiToon.Api.Application.Reviews.Commands.WriteSeriesReviews;
using DashiToon.Api.Application.Reviews.Models;
using DashiToon.Api.Application.Reviews.Queries.GetCurrentUserReview;
using DashiToon.Api.Application.Reviews.Queries.GetSeriesReviews;
using DashiToon.Api.Application.SeriesDetail.Queries.GetOwnedChapters;
using DashiToon.Api.Application.SeriesDetail.Queries.GetRelatedSeries;
using DashiToon.Api.Application.SeriesDetail.Queries.GetSeriesAnalytics;
using DashiToon.Api.Application.SeriesDetail.Queries.GetSeriesChapters;
using DashiToon.Api.Application.SeriesDetail.Queries.GetSeriesDashiFans;
using DashiToon.Api.Application.SeriesDetail.Queries.GetSeriesDetail;
using DashiToon.Api.Application.SeriesDetail.Queries.GetSeriesVolumes;
using DashiToon.Api.Application.Users.Commands.FollowSeries;
using DashiToon.Api.Application.Users.Commands.SubscribeSeries;
using DashiToon.Api.Application.Users.Commands.UnfollowSeriesCommand;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DashiToon.Api.Web.Endpoints;

public class Series : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(GetSeriesDetail, "{id}")
            .MapGet(GetSeriesAnalytics, "{id}/analytics")
            .MapGet(GetRelatedSeries, "{id}/Related")
            .MapGet(GetSeriesDashiFans, "{id}/dashi-fans")
            .MapPost(SubscribeSeries, "{id}/dashi-fans/{tierId}/subscribe")
            .MapGet(GetSeriesVolumes, "{id}/volumes")
            .MapGet(GetSeriesChapters, "{id}/volumes/{volumeId}/chapters")
            .MapGet(GetSeriesOwnedChapters, "{id}/volumes/{volumeId}/owned-chapters");

        app.MapGroup(this)
            .MapGet(GetSeriesReviews, "{id}/reviews");

        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetCurrentUserReview, "{id}/reviews/current-user")
            .MapPost(WriteReview, "{id}/reviews")
            .MapPut(UpdateReview, "{id}/reviews/{reviewId}")
            .MapPost(RateReview, "{id}/reviews/{reviewId}/rate");

        app.MapGroup(this)
            .RequireAuthorization()
            .MapPost(FollowSeries, "{id}/follows")
            .MapDelete(UnfollowSeries, "{id}/follows");
    }

    public async Task<SeriesDetailVm> GetSeriesDetail(ISender sender, int id)
    {
        return await sender.Send(new GetSeriesDetailQuery(id));
    }

    public async Task<SeriesAnalyticsVm> GetSeriesAnalytics(ISender sender, int id)
    {
        return await sender.Send(new GetSeriesAnalyticsQuery(id));
    }

    public async Task<List<RelatedSeriesVm>> GetRelatedSeries(ISender sender, int id)
    {
        return await sender.Send(new GetRelatedSeriesQuery(id));
    }

    public async Task<List<SeriesDashiFanVm>> GetSeriesDashiFans(ISender sender, int id)
    {
        return await sender.Send(new GetSeriesDashiFansQuery(id));
    }

    public async Task<IResult> SubscribeSeries(ISender sender, int id, Guid tierId, SubscribeSeriesCommand command)
    {
        if (id != command.SeriesId || tierId != command.TierId)
        {
            return Results.BadRequest();
        }

        return Results.Ok(await sender.Send(command));
    }

    public async Task<List<SeriesVolumeVm>> GetSeriesVolumes(ISender sender, int id)
    {
        return await sender.Send(new GetSeriesVolumesQuery(id));
    }

    public async Task<List<SeriesChapterVm>> GetSeriesChapters(ISender sender, int id, int volumeId)
    {
        return await sender.Send(new GetSeriesChaptersQuery(id, volumeId));
    }

    public async Task<List<OwnedChapterVm>> GetSeriesOwnedChapters(ISender sender, int id, int volumeId)
    {
        return await sender.Send(new GetOwnedChaptersQuery(id, volumeId));
    }

    public async Task<PaginatedList<ReviewVm>> GetSeriesReviews(
        ISender sender,
        int id,
        string sortBy = "Relevance",
        int pageNumber = 1,
        int pageSize = 10)
    {
        return await sender.Send(new GetSeriesReviewQuery(id, pageNumber, pageSize, sortBy));
    }

    public async Task<ReviewVm?> GetCurrentUserReview(ISender sender, int id)
    {
        return await sender.Send(new GetCurrentUserReviewQuery(id));
    }

    public async Task<Results<BadRequest, Ok<ReviewVm>>> WriteReview(
        ISender sender,
        int id,
        WriteSeriesReviewCommand command)
    {
        if (id != command.SeriesId)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(await sender.Send(command));
    }

    public async Task<Results<BadRequest, Ok<ReviewVm>>> UpdateReview(
        ISender sender,
        int id,
        Guid reviewId,
        UpdateReviewCommand command)
    {
        if (id != command.SeriesId || reviewId != command.ReviewId)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(await sender.Send(command));
    }

    public async Task<Results<BadRequest, Ok<ReviewVm>>> RateReview(
        ISender sender,
        int id,
        Guid reviewId,
        RateReviewCommand command)
    {
        if (id != command.SeriesId || reviewId != command.ReviewId)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(await sender.Send(command));
    }

    public async Task<IResult> FollowSeries(
        ISender sender,
        int id,
        FollowSeriesCommand command)
    {
        if (id != command.SeriesId)
        {
            return TypedResults.BadRequest();
        }

        await sender.Send(command);

        return Results.Ok();
    }

    public async Task<IResult> UnfollowSeries(ISender sender, int id)
    {
        await sender.Send(new UnfollowSeriesCommand(id));

        return Results.NoContent();
    }
}
