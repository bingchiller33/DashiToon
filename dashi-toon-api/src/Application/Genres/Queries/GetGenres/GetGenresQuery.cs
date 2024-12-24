using DashiToon.Api.Application.Common.Interfaces;

namespace DashiToon.Api.Application.Genres.Queries.GetGenres;

public record GetGenresQuery : IRequest<List<GenreVm>>;

public class GetGenresQueryHandler : IRequestHandler<GetGenresQuery, List<GenreVm>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetGenresQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<GenreVm>> Handle(GetGenresQuery request, CancellationToken cancellationToken)
    {
        return _dbContext.Genres
            .Select(x => new GenreVm(x.Id, x.Name))
            .ToListAsync(cancellationToken);
    }
}
