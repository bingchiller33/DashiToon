using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.ReadContent.Commands.SyncViewCount;

public sealed record SyncViewCountCommand(Dictionary<int, int> ViewAnalytics) : IRequest;

public sealed class SyncViewCountCommandHandler : IRequestHandler<SyncViewCountCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISearchService _searchService;

    public SyncViewCountCommandHandler(IApplicationDbContext context, ISearchService searchService)
    {
        _context = context;
        _searchService = searchService;
    }

    public async Task Handle(SyncViewCountCommand request, CancellationToken cancellationToken)
    {
        List<int>? chapterIds = request.ViewAnalytics.Keys.ToList();

        List<Chapter>? chapters = _context.Chapters
            .Include(c => c.Volume)
            .Where(c => chapterIds.Contains(c.Id))
            .ToList();

        Dictionary<int, int>? seriesIncrementViewCount = new();

        foreach (Chapter? chapter in chapters)
        {
            if (!request.ViewAnalytics.TryGetValue(chapter.Id, out int viewCount))
            {
                continue;
            }

            chapter.AddReadingAnalytic(viewCount);

            if (seriesIncrementViewCount.TryGetValue(chapter.Volume.SeriesId, out int incrementViewCount))
            {
                seriesIncrementViewCount[chapter.Volume.SeriesId] += viewCount;
            }
            else
            {
                seriesIncrementViewCount[chapter.Volume.SeriesId] = viewCount;
            }
        }

        await _searchService.BulkUpdateViewCountAsync(seriesIncrementViewCount);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
