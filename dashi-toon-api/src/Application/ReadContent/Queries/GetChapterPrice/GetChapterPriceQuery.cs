using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.ReadContent.Queries.Models;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.ReadContent.Queries.GetChapterPrice;

[Authorize]
public sealed record GetChapterPriceQuery(int SeriesId, int VolumeId, int ChapterId) : IRequest<int?>;

public sealed class GetChapterPriceQueryHandler : IRequestHandler<GetChapterPriceQuery, int?>
{
    private readonly IChapterRepository _chapterRepository;
    private readonly IApplicationDbContext _dbContext;

    public GetChapterPriceQueryHandler(IApplicationDbContext dbContext, IChapterRepository chapterRepository)
    {
        _dbContext = dbContext;
        _chapterRepository = chapterRepository;
    }

    public async Task<int?> Handle(GetChapterPriceQuery request, CancellationToken cancellationToken)
    {
        Series? series = await _dbContext.Series
            .Include(s => s.Volumes.Where(v => v.Id == request.VolumeId))
            .FirstOrDefaultAsync(s => s.Id == request.SeriesId
                                      && s.Status != SeriesStatus.Hiatus
                                      && s.Status != SeriesStatus.Trashed
                                      && s.Status != SeriesStatus.Draft, cancellationToken);

        if (series is null)
        {
            throw new NotFoundException(request.SeriesId.ToString(), nameof(Series));
        }

        if (!series.Volumes.Any())
        {
            throw new NotFoundException(request.VolumeId.ToString(), nameof(Volume));
        }

        ChapterContent? chapter = await _chapterRepository
            .FindPublishedChapterById(request.VolumeId, request.ChapterId);

        if (chapter is null)
        {
            throw new NotFoundException(request.ChapterId.ToString(), nameof(Chapter));
        }

        return chapter.KanaPrice;
    }
}
