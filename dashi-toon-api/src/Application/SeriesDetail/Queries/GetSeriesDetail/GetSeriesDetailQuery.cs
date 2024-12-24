using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.SeriesDetail.Queries.GetSeriesDetail;

public sealed record GetSeriesDetailQuery(int Id) : IRequest<SeriesDetailVm>;

public sealed class GetSeriesDetailQueryHandler : IRequestHandler<GetSeriesDetailQuery, SeriesDetailVm>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IImageStore _imageStore;

    public GetSeriesDetailQueryHandler(IApplicationDbContext dbContext, IImageStore imageStore)
    {
        _dbContext = dbContext;
        _imageStore = imageStore;
    }


    public async Task<SeriesDetailVm> Handle(GetSeriesDetailQuery request, CancellationToken cancellationToken)
    {
        Series? series = await _dbContext.Series
            .Include(s => s.Genres)
            .FirstOrDefaultAsync(s => s.Id == request.Id
                                      && s.Status != SeriesStatus.Hiatus &&
                                      s.Status != SeriesStatus.Trashed &&
                                      s.Status != SeriesStatus.Draft,
                cancellationToken);

        if (series is null)
        {
            throw new NotFoundException(request.Id.ToString(), nameof(Series));
        }

        return new SeriesDetailVm(
            series.Id,
            series.Title,
            series.AlternativeTitles,
            string.Join(", ", series.Authors),
            series.Status,
            series.Synopsis,
            string.IsNullOrEmpty(series.Thumbnail)
                ? await _imageStore.GetUrl("thumbnails/default.png", DateTime.UtcNow.AddMinutes(2))
                : await _imageStore.GetUrl($"thumbnails/{series.Thumbnail}", DateTime.UtcNow.AddMinutes(2)),
            series.Type,
            series.Genres.Select(g => new LookupDto { Id = g.Id, Title = g.Name }).ToList(),
            series.ContentRating
        );
    }
}
