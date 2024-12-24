using DashiToon.Api.Application.ReadContent.Commands.UnlockChapter;
using DashiToon.Api.Application.ReadContent.Queries.GetChapterPrice;
using DashiToon.Api.Application.ReadContent.Queries.GetComicChapter;
using DashiToon.Api.Application.ReadContent.Queries.GetNovelChapter;

namespace DashiToon.Api.Web.Endpoints;

public class ReadContent : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(ReadComicChapter, "series/{id}/volumes/{volumeId}/chapters/{chapterId}/comic")
            .MapGet(ReadNovelChapter, "series/{id}/volumes/{volumeId}/chapters/{chapterId}/novel");

        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetChapterPrice, "series/{id}/volumes/{volumeId}/chapters/{chapterId}/price")
            .MapPost(UnlockChapter, "series/{id}/volumes/{volumeId}/chapters/{chapterId}/unlock");
    }


    public async Task<ComicChapterDetailVm> ReadComicChapter(ISender sender, int id, int volumeId, int chapterId)
    {
        return await sender.Send(new GetComicChapterQuery(id, volumeId, chapterId));
    }

    public async Task<NovelChapterDetailVm> ReadNovelChapter(ISender sender, int id, int volumeId, int chapterId)
    {
        return await sender.Send(new GetNovelChapterQuery(id, volumeId, chapterId));
    }

    public async Task<int?> GetChapterPrice(
        ISender sender,
        int id,
        int volumeId,
        int chapterId)
    {
        return await sender.Send(new GetChapterPriceQuery(id, volumeId, chapterId));
    }

    public async Task<IResult> UnlockChapter(
        ISender sender,
        int id,
        int volumeId,
        int chapterId,
        UnlockChapterCommand command)
    {
        if (id != command.SeriesId || volumeId != command.VolumeId || chapterId != command.ChapterId)
        {
            return Results.BadRequest();
        }

        await sender.Send(command);

        return Results.NoContent();
    }
}
