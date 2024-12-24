using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;

namespace DashiToon.Api.Application.AuthorStudio.Series.Queries.GetSeries;

[Authorize]
public sealed record GetSeriesQuery(int Id) : IRequest<SeriesDetailVm>;

public sealed class GetSeriesQueryHandler : IRequestHandler<GetSeriesQuery, SeriesDetailVm>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;

    public GetSeriesQueryHandler(IApplicationDbContext dbContext, IUser user)
    {
        _dbContext = dbContext;
        _user = user;
    }

    public async Task<SeriesDetailVm> Handle(GetSeriesQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.Series? series = await _dbContext.Series
            .Include(x => x.Genres)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (series is null)
        {
            throw new NotFoundException(request.Id.ToString(), nameof(Series));
        }

        if (series.CreatedBy != _user.Id)
        {
            throw new ForbiddenAccessException();
        }

        return new SeriesDetailVm(
            series.Id,
            series.Title,
            series.AlternativeTitles,
            series.Authors,
            series.StartTime.HasValue ? series.StartTime.Value.ToString("O") : string.Empty,
            series.Status,
            series.Synopsis,
            series.Thumbnail ?? string.Empty,
            series.Type,
            series.Genres.Select(s => s.Name).ToList(),
            series.ContentRating,
            series.CategoryRatings.Select(s => new CategoryRatingVm(s.Category.ToString(), s.Rating)).ToList()
        );
    }
}
