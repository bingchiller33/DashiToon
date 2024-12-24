using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Genres.Queries.GetGenres;

namespace DashiToon.Api.Application.Homepage.Queries.GetTopGenres;

public sealed record GetTopGenresQuery : IRequest<List<GenreVm>>;

public sealed class GetTopGenresQueryHandler : IRequestHandler<GetTopGenresQuery, List<GenreVm>>
{
    private readonly IAnalyticRepository _analyticRepository;

    public GetTopGenresQueryHandler(IAnalyticRepository analyticRepository)
    {
        _analyticRepository = analyticRepository;
    }

    public async Task<List<GenreVm>> Handle(GetTopGenresQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<GenreDto>? genres = await _analyticRepository.GetTopGenres();

        return genres.Select(g => new GenreVm(g.Id, g.Name)).ToList();
    }
}
