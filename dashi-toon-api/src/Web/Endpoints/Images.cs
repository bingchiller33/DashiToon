using DashiToon.Api.Application.AuthorStudio.Chapters.Models;
using DashiToon.Api.Application.Images.Commands.DeleteImage;
using DashiToon.Api.Application.Images.Queries.GetImageUrl;

namespace DashiToon.Api.Web.Endpoints;

public class Images : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app
            .MapGroup(this)
            .MapGet(GetImageUrl, "{fileName}")
            .MapDelete(DeleteImage, "{fileName}");
    }

    public async Task<ImageInfo> GetImageUrl(ISender sender, string fileName, string type)
    {
        return await sender.Send(new GetImageUrlQuery(fileName, type));
    }

    public async Task<IResult> DeleteImage(ISender sender, string fileName, string type)
    {
        await sender.Send(new DeleteImageCommand(fileName, type));

        return Results.Ok();
    }
}
