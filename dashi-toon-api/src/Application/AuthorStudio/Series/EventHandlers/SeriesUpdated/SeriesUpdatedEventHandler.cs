using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using DashiToon.Api.Domain.Events;

namespace DashiToon.Api.Application.AuthorStudio.Series.EventHandlers.SeriesUpdated;

public sealed class SeriesUpdatedEventHandler : INotificationHandler<SeriesUpdatedEvent>
{
    private readonly ISearchService _searchService;
    private readonly IModerationService _moderationService;
    private readonly IApplicationDbContext _context;
    private readonly IImageStore _imageStore;

    public SeriesUpdatedEventHandler(
        ISearchService searchService,
        IModerationService moderationService,
        IApplicationDbContext context,
        IImageStore imageStore)
    {
        _searchService = searchService;
        _moderationService = moderationService;
        _context = context;
        _imageStore = imageStore;
    }

    public async Task Handle(SeriesUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await _searchService.UpdateSeriesAsync(notification.Series);

        if (notification.Series.Status is SeriesStatus.Draft or SeriesStatus.Hiatus or SeriesStatus.Trashed)
        {
            return;
        }

        string? thumbnailUrl = string.IsNullOrEmpty(notification.Series.Thumbnail)
            ? null
            : await _imageStore.GetUrl($"thumbnails/{notification.Series.Thumbnail}", DateTime.UtcNow.AddMinutes(2));

        ModerationAnalysis? analysis = await _moderationService.ModerateSeries(
            notification.Series.Title,
            notification.Series.Synopsis,
            thumbnailUrl);

        if (!analysis.Flagged)
        {
            return;
        }

        Report? report = Report.CreateNewSystemReport(
            ReportType.Series,
            notification.Series.Id
        );

        report.AddAnalytics(analysis);

        _context.Reports.Add(report);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
