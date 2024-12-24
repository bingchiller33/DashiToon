using DashiToon.Api.Application.Genres.Queries.GetGenres;

namespace DashiToon.Api.Web.Endpoints;

public class Genres : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app
            .MapGroup(this)
            .MapGet(GetGenres);
    }

    public async Task<List<GenreVm>> GetGenres(ISender sender)
    {
        return await sender.Send(new GetGenresQuery());
    }
}
