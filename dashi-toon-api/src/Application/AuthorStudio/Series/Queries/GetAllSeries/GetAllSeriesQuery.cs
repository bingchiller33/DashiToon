using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;

namespace DashiToon.Api.Application.AuthorStudio.Series.Queries.GetAllSeries;

[Authorize]
public sealed record GetAllSeriesQuery : IRequest<List<SeriesVm>>;

public sealed class GetAllSeriesQueryHandler : IRequestHandler<GetAllSeriesQuery, List<SeriesVm>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;

    public GetAllSeriesQueryHandler(IApplicationDbContext dbContext, IUser user)
    {
        _dbContext = dbContext;
        _user = user;
    }

    public async Task<List<SeriesVm>> Handle(GetAllSeriesQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Series
            .Include(x => x.Genres)
            .Where(x => x.CreatedBy == _user.Id)
            .Select(x => new SeriesVm(
                x.Id,
                x.Title,
                x.AlternativeTitles,
                x.Authors,
                x.StartTime.HasValue ? x.StartTime.Value.ToString("O") : string.Empty,
                x.Status,
                x.Thumbnail ?? string.Empty,
                x.Type,
                x.Genres.Select(g => g.Name).ToList(),
                x.ContentRating,
                x.LastModified.ToLocalTime().ToString("O")))
            .ToListAsync(cancellationToken);
    }
}
