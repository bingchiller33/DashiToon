using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;

[Authorize]
[Restrict(Require = Restrictions.NotRestricted)]
public sealed record CreateSeriesCommand(
    string Title,
    string Synopsis,
    string Thumbnail,
    SeriesType SeriesType,
    int[] Genres,
    ContentQuestionnaire[] CategoryRatings,
    string[] AlternativeTitles,
    string[] Authors,
    DateTimeOffset? StartTime
) : IRequest<int>;

public sealed record ContentQuestionnaire(ContentCategory Category, int Option);

public sealed class CreateSeriesCommandHandler : IRequestHandler<CreateSeriesCommand, int>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _dbContext;
    private readonly ISearchService _searchService;
    private readonly IUser _user;

    public CreateSeriesCommandHandler(
        IIdentityService identityService,
        IApplicationDbContext dbContext,
        ISearchService searchService,
        IUser user)
    {
        _identityService = identityService;
        _dbContext = dbContext;
        _searchService = searchService;
        _user = user;
    }

    public async Task<int> Handle(CreateSeriesCommand request, CancellationToken cancellationToken)
    {
        Genre[] genres = await _dbContext.Genres
            .Where(x => request.Genres.Contains(x.Id))
            .ToArrayAsync(cancellationToken);

        CategoryRating[] categoryRatings = request.CategoryRatings
            .Select(x => CategoryRating.Create(x.Category, x.Option))
            .ToArray();

        string[]? authors = request.Authors;

        if (authors.Length == 0)
        {
            string? username = await _identityService.GetUserNameAsync(_user.Id!);

            if (username is not null)
            {
                authors = [username];
            }
        }

        Domain.Entities.Series series = Domain.Entities.Series.CreateNew(
            request.Title,
            request.Synopsis,
            request.Thumbnail,
            request.SeriesType,
            genres,
            categoryRatings,
            request.AlternativeTitles,
            authors,
            request.StartTime);

        await _dbContext.Series.AddAsync(series, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _searchService.IndexSeriesAsync(series);

        return series.Id;
    }
}
