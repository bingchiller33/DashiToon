using DashiToon.Api.Application.AuthorStudio.Analytics.Models;
using DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetChapterCommentRankings;
using DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetChapterKanaRankings;
using DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetChapterViewRankings;
using DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesCommentAnalytics;
using DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesDashiFanAnalytics;
using DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesDashiFanRankings;
using DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesGeneralAnalytis;
using DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesKanaAnalytics;
using DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesReviewAnalytics;
using DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesViewAnalytics;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.BulkDeleteChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.BulkPublishChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateComicChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.DeleteChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.DeleteChapterVersion;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.PublishChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.ReorderChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.RestoreChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.SetChapterPricing;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.UnpublishChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.UpdateChapterVersion;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.UpdateComicChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.UpdateNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.UploadImages;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.UploadThumbnail;
using DashiToon.Api.Application.AuthorStudio.Chapters.Queries.GetChapters;
using DashiToon.Api.Application.AuthorStudio.Chapters.Queries.GetComicChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Queries.GetComicChapterVersions;
using DashiToon.Api.Application.AuthorStudio.Chapters.Queries.GetNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Queries.GetNovelChapterVersions;
using DashiToon.Api.Application.AuthorStudio.Chapters.Queries.PreviewComicChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Queries.PreviewNovelChapter;
using DashiToon.Api.Application.AuthorStudio.DashiFans.Commands.CreateDashiFan;
using DashiToon.Api.Application.AuthorStudio.DashiFans.Commands.UpdateDashiFan;
using DashiToon.Api.Application.AuthorStudio.DashiFans.Commands.UpdateDashiFanStatus;
using DashiToon.Api.Application.AuthorStudio.DashiFans.Queries.GetDashiFans;
using DashiToon.Api.Application.AuthorStudio.Revenue.Commands.WithdrawRevenue;
using DashiToon.Api.Application.AuthorStudio.Revenue.Models;
using DashiToon.Api.Application.AuthorStudio.Revenue.Queries.GetRevenue;
using DashiToon.Api.Application.AuthorStudio.Revenue.Queries.GetRevenueTransactions;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.DeleteSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UploadThumbnail;
using DashiToon.Api.Application.AuthorStudio.Series.Queries.GetAllSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Queries.GetSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.DeleteVolume;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.UpdateVolume;
using DashiToon.Api.Application.AuthorStudio.Volumes.Queries.GetVolume;
using DashiToon.Api.Application.AuthorStudio.Volumes.Queries.GetVolumes;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Images.Commands.UploadImage;
using DashiToon.Api.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DashiToon.Api.Web.Endpoints;

public class AuthorStudio : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app
            .MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetAllSeries, "series")
            .MapGet(GetSeries, "series/{id}")
            .MapPost(CreateNewSeries, "series")
            .MapPut(UpdateSeries, "series/{id}")
            .MapDelete(DeleteSeries, "series/{id}");

        app
            .MapGroup(this)
            .RequireAuthorization()
            .DisableAntiforgery()
            .MapPost(UploadSeriesThumbnail, "series/thumbnails")
            .MapPost(UploadChapterThumbnail, "series/chapters/thumbnails")
            .MapPost(UploadChapterImages, "series/chapters");

        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetAllVolumes, "series/{id}/volumes")
            .MapGet(GetVolume, "series/{id}/volumes/{volumeId}")
            .MapPost(CreateNewVolume, "series/{id}/volumes")
            .MapPut(UpdateVolume, "series/{id}/volumes/{volumeId}")
            .MapDelete(DeleteVolume, "series/{id}/volumes/{volumeId}");

        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetNovelChapter, "series/{id}/volumes/{volumeId}/chapters/{chapterId}/novel")
            .MapGet(GetNovelChapterPreview,
                "series/{id}/volumes/{volumeId}/chapters/{chapterId}/novel/versions/{versionId}/preview")
            .MapGet(GetComicChapter, "series/{id}/volumes/{volumeId}/chapters/{chapterId}/comic")
            .MapGet(GetComicChapterPreview,
                "series/{id}/volumes/{volumeId}/chapters/{chapterId}/comic/versions/{versionId}/preview")
            .MapGet(GetAllChapters, "series/{id}/volumes/{volumeId}/chapters")
            .MapPost(CreateNovelChapter, "series/{id}/volumes/{volumeId}/chapters/novel")
            .MapPost(CreateComicChapter, "series/{id}/volumes/{volumeId}/chapters/comic")
            .MapPut(UpdateNovelChapter, "series/{id}/volumes/{volumeId}/chapters/{chapterId}/novel")
            .MapPut(UpdateComicChapter, "series/{id}/volumes/{volumeId}/chapters/{chapterId}/comic")
            .MapPut(SetChapterPrice, "series/{id}/volumes/{volumeId}/chapters/{chapterId}/price")
            .MapPut(PublishChapter, "series/{id}/volumes/{volumeId}/chapters/{chapterId}/publish")
            .MapPut(UnpublishChapter, "series/{id}/volumes/{volumeId}/chapters/{chapterId}/unpublish")
            .MapPut(BulkPublishChapter, "series/{id}/volumes/{volumeId}/chapters/bulk-publish")
            .MapDelete(DeleteChapter, "series/{id}/volumes/{volumeId}/chapters/{chapterId}")
            .MapPut(BulkDeleteChapter, "series/{id}/volumes/{volumeId}/chapters/bulk-delete")
            .MapPut(ReorderChapter, "series/{id}/volumes/{volumeId}/chapters/{chapterId}/reorder")
            .MapPut(RestoreChapter, "series/{id}/volumes/{volumeId}/chapters/{chapterId}/restore-version");

        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetComicChapterVersions, "series/{id}/volumes/{volumeId}/chapters/{chapterId}/comic/versions")
            .MapGet(GetNovelChapterVersions, "series/{id}/volumes/{volumeId}/chapters/{chapterId}/novel/versions")
            .MapPut(UpdateChapterVersion, "series/{id}/volumes/{volumeId}/chapters/{chapterId}/versions/{versionId}")
            .MapDelete(DeleteChapterVersion,
                "series/{id}/volumes/{volumeId}/chapters/{chapterId}/versions/{versionId}");

        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetAllDashiFanTiers, "series/{id}/dashi-fans")
            .MapPost(CreateNewDashiFanTier, "series/{id}/dashi-fans")
            .MapPut(UpdateTier, "series/{id}/dashi-fans/{tierId}")
            .MapPut(UpdateTierStatus, "series/{id}/dashi-fans/{tierId}/status");

        app.MapGroup(this)
            .RequireAuthorization()
            .MapPost(WithdrawRevenue, "revenue/withdraw")
            .MapGet(GetRevenue, "revenue")
            .MapGet(GetRevenueTransactions, "revenue/transactions");

        app.MapGroup(this)
            .RequireAuthorization()
            .MapPost(GetSeriesGeneralAnalytics, "series/{id}/analytics/general")
            .MapPost(GetSeriesViewAnalytics, "series/{id}/analytics/view")
            .MapPost(GetSeriesChapterViewRankingsAnalytics, "series/{id}/analytics/chapter/view-rankings")
            .MapPost(GetSeriesCommentAnalytics, "series/{id}/analytics/comment")
            .MapPost(GetSeriesChapterCommentRankingsAnalytics, "series/{id}/analytics/chapter/comment-rankings")
            .MapPost(GetSeriesReviewAnalytics, "series/{id}/analytics/review")
            .MapPost(GetSeriesKanaAnalytics, "series/{id}/analytics/kana")
            .MapPost(GetChapterKanaRankingsAnalytics, "series/{id}/analytics/chapter/kana-rankings")
            .MapPost(GetSeriesDashiFanAnalytics, "series/{id}/analytics/dashi-fan")
            .MapPost(GetSeriesDashiFanRankingsAnalytics, "series/{id}/analytics/dashi-fan-rankings")
            ;
    }

    public async Task<List<SeriesVm>> GetAllSeries(ISender sender)
    {
        return await sender.Send(new GetAllSeriesQuery());
    }

    public async Task<SeriesDetailVm> GetSeries(ISender sender, int id)
    {
        return await sender.Send(new GetSeriesQuery(id));
    }

    public async Task<int> CreateNewSeries(ISender sender, CreateSeriesCommand command)
    {
        return await sender.Send(command);
    }

    public async Task<IResult> UpdateSeries(ISender sender, int id, UpdateSeriesCommand command)
    {
        if (id != command.Id)
        {
            return Results.BadRequest();
        }

        await sender.Send(command);

        return Results.NoContent();
    }

    public async Task<IResult> DeleteSeries(ISender sender, int id)
    {
        await sender.Send(new DeleteSeriesCommand(id));

        return Results.NoContent();
    }

    public async Task<UploadImageResponse?> UploadSeriesThumbnail(ISender sender, IFormFile file)
    {
        return await sender.Send(new UploadSeriesThumbnailCommand(
            file.FileName,
            file.Length,
            file.ContentType,
            file.OpenReadStream()));
    }

    public async Task<UploadImageResponse?> UploadChapterThumbnail(ISender sender, IFormFile file)
    {
        return await sender.Send(new UploadChapterThumbnailCommand(
            file.FileName,
            file.Length,
            file.ContentType,
            file.OpenReadStream()));
    }

    public async Task<UploadImageResponse?> UploadChapterImages(ISender sender, IFormFile file)
    {
        return await sender.Send(new UploadChapterImageCommand(
            file.FileName,
            file.Length,
            file.ContentType,
            file.OpenReadStream()
        ));
    }

    public async Task<List<VolumeVm>> GetAllVolumes(ISender sender, int id)
    {
        return await sender.Send(new GetVolumesQuery(id));
    }

    public async Task<VolumeDetailVm> GetVolume(ISender sender, int id, int volumeId)
    {
        return await sender.Send(new GetVolumeQuery(id, volumeId));
    }

    public async Task<IResult> CreateNewVolume(ISender sender, int id, CreateVolumeCommand command)
    {
        if (id != command.SeriesId)
        {
            return Results.BadRequest();
        }

        int result = await sender.Send(command);

        return Results.Ok(result);
    }

    public async Task<IResult> UpdateVolume(ISender sender, int id, int volumeId, UpdateVolumeCommand command)
    {
        if (id != command.SeriesId || volumeId != command.VolumeId)
        {
            return Results.BadRequest();
        }

        await sender.Send(command);

        return Results.NoContent();
    }

    public async Task<IResult> DeleteVolume(ISender sender, int id, int volumeId)
    {
        await sender.Send(new DeleteVolumeCommand(id, volumeId));

        return Results.NoContent();
    }

    public async Task<IResult> GetAllChapters(
        ISender sender,
        int id,
        int volumeId,
        string? title,
        ChapterStatus? status,
        int pageNumber,
        int pageSize)
    {
        PaginatedList<ChapterVm> result =
            await sender.Send(new GetChaptersQuery(id, volumeId, title, status, pageNumber, pageSize));

        return Results.Ok(result);
    }

    public async Task<IResult> CreateNovelChapter(
        ISender sender,
        int id,
        int volumeId,
        CreateNovelChapterCommand command)
    {
        if (id != command.SeriesId || volumeId != command.VolumeId)
        {
            return Results.BadRequest();
        }

        int result = await sender.Send(command);

        return Results.Ok(result);
    }

    public async Task<IResult> CreateComicChapter(
        ISender sender,
        int id,
        int volumeId,
        CreateComicChapterCommand command)
    {
        if (id != command.SeriesId || volumeId != command.VolumeId)
        {
            return Results.BadRequest();
        }

        int result = await sender.Send(command);

        return Results.Ok(result);
    }

    public async Task<IResult> UpdateComicChapter(
        ISender sender,
        int id,
        int volumeId,
        int chapterId,
        UpdateComicChapterCommand command)
    {
        if (id != command.SeriesId || volumeId != command.VolumeId || chapterId != command.ChapterId)
        {
            return Results.BadRequest();
        }

        await sender.Send(command);

        return Results.NoContent();
    }

    public async Task<IResult> UpdateNovelChapter(
        ISender sender,
        int id,
        int volumeId,
        int chapterId,
        UpdateNovelChapterCommand command)
    {
        if (id != command.SeriesId || volumeId != command.VolumeId || chapterId != command.ChapterId)
        {
            return Results.BadRequest();
        }

        await sender.Send(command);

        return Results.NoContent();
    }

    public async Task<IResult> SetChapterPrice(
        ISender sender,
        int id,
        int volumeId,
        int chapterId,
        SetChapterPricingCommand command)
    {
        if (id != command.SeriesId || volumeId != command.VolumeId || chapterId != command.ChapterId)
        {
            return Results.BadRequest();
        }

        await sender.Send(command);

        return Results.NoContent();
    }

    public async Task<IResult> PublishChapter(
        ISender sender,
        int id,
        int volumeId,
        int chapterId,
        PublishChapterCommand command)
    {
        if (id != command.SeriesId || volumeId != command.VolumeId || chapterId != command.ChapterId)
        {
            return Results.BadRequest();
        }

        await sender.Send(command);

        return Results.NoContent();
    }

    public async Task<IResult> UnpublishChapter(
        ISender sender,
        int id,
        int volumeId,
        int chapterId)
    {
        await sender.Send(new UnpublishChapterCommand(id, volumeId, chapterId));

        return Results.NoContent();
    }

    public async Task<IResult> DeleteChapter(ISender sender, int id, int volumeId, int chapterId)
    {
        await sender.Send(new DeleteChapterCommand(id, volumeId, chapterId));

        return Results.NoContent();
    }

    public async Task<IResult> BulkPublishChapter(ISender sender, int id, int volumeId,
        BulkPublishChapterCommand command)
    {
        if (id != command.SeriesId || volumeId != command.VolumeId)
        {
            return Results.BadRequest();
        }

        await sender.Send(command);

        return Results.NoContent();
    }

    public async Task<NovelChapterDetailVm> GetNovelChapter(ISender sender, int id, int volumeId, int chapterId)
    {
        return await sender.Send(new GetNovelChapterQuery(id, volumeId, chapterId));
    }

    public async Task<ComicChapterDetailVm> GetComicChapter(ISender sender, int id, int volumeId, int chapterId)
    {
        return await sender.Send(new GetComicChapterQuery(id, volumeId, chapterId));
    }

    public async Task<IResult> BulkDeleteChapter(ISender sender, int id, int volumeId, BulkDeleteChapterCommand command)
    {
        await sender.Send(command);

        return Results.NoContent();
    }

    public async Task<IResult> ReorderChapter(
        ISender sender,
        int id,
        int volumeId,
        int chapterId,
        ReorderChapterCommand command)
    {
        if (id != command.SeriesId || volumeId != command.VolumeId || chapterId != command.ChapterId)
        {
            return Results.BadRequest();
        }

        await sender.Send(command);

        return Results.NoContent();
    }

    public async Task<IResult> RestoreChapter(
        ISender sender,
        int id,
        int volumeId,
        int chapterId,
        RestoreChapterCommand command)
    {
        if (id != command.SeriesId || volumeId != command.VolumeId || chapterId != command.ChapterId)
        {
            return Results.BadRequest();
        }

        await sender.Send(command);

        return Results.NoContent();
    }

    public async Task<PaginatedList<ComicChapterVersionVm>> GetComicChapterVersions(
        ISender sender,
        int id,
        int volumeId,
        int chapterId,
        string? versionName,
        bool includeAutoSave,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int pageNumber = 1,
        int pageSize = 10)
    {
        return await sender.Send(new GetComicChapterVersionsQuery(
            id,
            volumeId,
            chapterId,
            versionName,
            includeAutoSave,
            from,
            to,
            pageNumber,
            pageSize));
    }

    public async Task<PaginatedList<NovelChapterVersionVm>> GetNovelChapterVersions(
        ISender sender,
        int id,
        int volumeId,
        int chapterId,
        string? versionName,
        bool includeAutoSave,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int pageNumber = 1,
        int pageSize = 10)
    {
        return await sender.Send(new GetNovelChapterVersionsQuery(
            id,
            volumeId,
            chapterId,
            versionName,
            includeAutoSave,
            from,
            to,
            pageNumber,
            pageSize));
    }

    public async Task<IResult> GetNovelChapterPreview(
        ISender sender,
        int id,
        int volumeId,
        int chapterId,
        Guid versionId)
    {
        return Results.Ok(await sender.Send(new PreviewNovelChapterQuery(id, volumeId, chapterId, versionId)));
    }

    public async Task<IResult> GetComicChapterPreview(
        ISender sender,
        int id,
        int volumeId,
        int chapterId,
        Guid versionId)
    {
        return Results.Ok(await sender.Send(new PreviewComicChapterQuery(id, volumeId, chapterId, versionId)));
    }

    public async Task<IResult> UpdateChapterVersion(
        ISender sender,
        int id,
        int volumeId,
        int chapterId,
        Guid versionId,
        UpdateChapterVersionCommand command)
    {
        if (id != command.SeriesId || volumeId != command.VolumeId || chapterId != command.ChapterId ||
            versionId != command.VersionId)
        {
            return Results.BadRequest();
        }

        await sender.Send(command);

        return Results.NoContent();
    }

    public async Task<IResult> DeleteChapterVersion(
        ISender sender,
        int id,
        int volumeId,
        int chapterId,
        Guid versionId)
    {
        await sender.Send(new DeleteChapterVersionCommand(id, volumeId, chapterId, versionId));

        return Results.NoContent();
    }

    public async Task<List<DashiFanVm>> GetAllDashiFanTiers(ISender sender, int id)
    {
        return await sender.Send(new GetDashiFansQuery(id));
    }

    public async Task<IResult> CreateNewDashiFanTier(ISender sender, int id, CreateDashiFanCommand command)
    {
        if (id != command.SeriesId)
        {
            return Results.BadRequest();
        }

        Guid result = await sender.Send(command);
        return Results.Ok(result);
    }

    public async Task<IResult> UpdateTier(
        ISender sender,
        int id,
        Guid tierId,
        UpdateDashiFanCommand command)
    {
        if (id != command.SeriesId || tierId != command.TierId)
        {
            return Results.BadRequest();
        }

        await sender.Send(command);
        return Results.NoContent();
    }

    public async Task<IResult> UpdateTierStatus(
        ISender sender,
        int id,
        Guid tierId,
        UpdateDashiFanStatusCommand command)
    {
        if (id != command.SeriesId || tierId != command.TierId)
        {
            return Results.BadRequest();
        }

        await sender.Send(command);
        return Results.NoContent();
    }

    public async Task<IResult> WithdrawRevenue(
        ISender sender,
        WithdrawKanaRevenueCommand command)
    {
        await sender.Send(command);

        return Results.NoContent();
    }

    public async Task<RevenueVm> GetRevenue(ISender sender)
    {
        return await sender.Send(new GetRevenueQuery());
    }

    public async Task<PaginatedList<RevenueTransactionVm>> GetRevenueTransactions(ISender sender,
        RevenueTransactionType type,
        int pageNumber = 1,
        int pageSize = 10)
    {
        return await sender.Send(new GetRevenueTransactionsQuery(type, pageNumber, pageSize));
    }

    public async Task<Results<Ok<GeneralAnalyticsVm>, BadRequest>> GetSeriesGeneralAnalytics(
        ISender sender,
        int id,
        GetSeriesGeneralAnalyticsQuery query)
    {
        if (id != query.SeriesId)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(await sender.Send(query));
    }

    public async Task<Results<Ok<ViewAnalyticsVm>, BadRequest>> GetSeriesViewAnalytics(
        ISender sender,
        int id,
        GetSeriesViewAnalyticsQuery query)
    {
        if (id != query.SeriesId)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(await sender.Send(query));
    }

    public async Task<Results<Ok<PaginatedList<ChapterRankings>>, BadRequest>> GetSeriesChapterViewRankingsAnalytics(
        ISender sender,
        int id,
        GetChapterViewRankingsQuery query)
    {
        if (id != query.SeriesId)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(await sender.Send(query));
    }

    public async Task<Results<Ok<CommentAnalyticsVm>, BadRequest>> GetSeriesCommentAnalytics(
        ISender sender,
        int id,
        GetSeriesCommentAnalyticsQuery query)
    {
        if (id != query.SeriesId)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(await sender.Send(query));
    }

    public async Task<Results<Ok<PaginatedList<ChapterRankings>>, BadRequest>> GetSeriesChapterCommentRankingsAnalytics(
        ISender sender,
        int id,
        GetChapterCommentRankingsQuery query)
    {
        if (id != query.SeriesId)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(await sender.Send(query));
    }

    public async Task<Results<Ok<ReviewAnalyticsVm>, BadRequest>> GetSeriesReviewAnalytics(
        ISender sender,
        int id,
        GetSeriesReviewAnalyticsQuery query)
    {
        if (id != query.SeriesId)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(await sender.Send(query));
    }

    public async Task<Results<Ok<PaginatedList<ChapterRankings>>, BadRequest>> GetChapterKanaRankingsAnalytics(
        ISender sender,
        int id,
        GetChapterKanaRankingsQuery query)
    {
        if (id != query.SeriesId)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(await sender.Send(query));
    }

    public async Task<Results<Ok<KanaAnalyticsVm>, BadRequest>> GetSeriesKanaAnalytics(
        ISender sender,
        int id,
        GetSeriesKanaAnalyticsQuery query)
    {
        if (id != query.SeriesId)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(await sender.Send(query));
    }

    public async Task<Results<Ok<DashiFanAnalyticsVm>, BadRequest>> GetSeriesDashiFanAnalytics(
        ISender sender,
        int id,
        GetSeriesDashiFanAnalyticsQuery query)
    {
        if (id != query.SeriesId)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(await sender.Send(query));
    }

    public async Task<Results<Ok<PaginatedList<DashiFanRanking>>, BadRequest>> GetSeriesDashiFanRankingsAnalytics(
        ISender sender,
        int id,
        GetSeriesDashiFanRankingsQuery query)
    {
        if (id != query.SeriesId)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(await sender.Send(query));
    }
}
