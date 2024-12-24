using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;

[Authorize]
[Restrict(Require = Restrictions.NotRestricted)]
public sealed record UpdateSeriesCommand(
    int Id,
    string Title,
    string Synopsis,
    string Thumbnail,
    SeriesStatus SeriesStatus,
    int[] Genres,
    ContentQuestionnaire[] CategoryRatings,
    string[] AlternativeTitles,
    string[] Authors,
    DateTimeOffset? StartTime) : IRequest;

public sealed class UpdateSeriesCommandHandler : IRequestHandler<UpdateSeriesCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;


    public UpdateSeriesCommandHandler(IApplicationDbContext dbContext, IUser user)
    {
        _dbContext = dbContext;
        _user = user;
    }

    public async Task Handle(UpdateSeriesCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Series? series = await _dbContext.Series
            .Include(x => x.Genres)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (series is null)
        {
            throw new NotFoundException(request.Id.ToString(), nameof(Domain.Entities.Series));
        }

        if (series.CreatedBy != _user.Id)
        {
            throw new ForbiddenAccessException();
        }

        Genre[] genres = await _dbContext.Genres
            .Where(x => request.Genres.Contains(x.Id))
            .ToArrayAsync(cancellationToken);

        CategoryRating[] categoryRatings = request.CategoryRatings
            .Select(x => CategoryRating.Create(x.Category, x.Option))
            .ToArray();

        series.Update(
            request.Title,
            request.Synopsis,
            request.Thumbnail,
            request.SeriesStatus,
            genres,
            categoryRatings,
            request.AlternativeTitles,
            request.Authors,
            request.StartTime);

        _dbContext.Series.Update(series);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
