using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Services;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using DashiToon.Api.Domain.Events;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.EventHandlers.ChapterPublished;

public class ChapterPublishedEventHandler : INotificationHandler<ChapterPublishedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IModerationService _moderationService;
    private readonly NovelChapterService _novelChapterService;
    private readonly ComicChapterService _comicChapterService;
    private readonly IImageStore _imageStore;

    public ChapterPublishedEventHandler(
        IApplicationDbContext context,
        IModerationService moderationService,
        NovelChapterService novelChapterService,
        ComicChapterService comicChapterService,
        IImageStore imageStore)
    {
        _context = context;
        _moderationService = moderationService;
        _novelChapterService = novelChapterService;
        _comicChapterService = comicChapterService;
        _imageStore = imageStore;
    }

    public async Task Handle(ChapterPublishedEvent publishedEvent, CancellationToken cancellationToken)
    {
        Volume volume = await _context.Volumes
            .Include(v => v.Series)
            .FirstAsync(v => v.Id == publishedEvent.Chapter.VolumeId, cancellationToken);

        List<string> followedUserIds = await _context.Follows.Where(f => f.SeriesId == volume.SeriesId)
            .Select(f => f.UserId).ToListAsync(cancellationToken);

        ChapterVersion? publishedVersion = publishedEvent.Chapter.GetPublishedVersion();

        string publishAnnouncement = publishedEvent.Chapter.PublishedDate > DateTimeOffset.UtcNow
            ? "đã được lên lịch xuất bản"
            : "đã được xuất bản";

        string notificationTitle = $"{volume.Series.Title} đã có chương mới";

        string notificationContent = $"""
                                      {volume.Series.Title} đã có chương mới
                                      [Chương {publishedEvent.Chapter.ChapterNumber} - {publishedVersion!.Title} Tập {volume.VolumeNumber}] đã {publishAnnouncement}
                                      """;

        _context.Notifications.AddRange(followedUserIds
            .Select(uId => Notification.Create(uId, notificationTitle, notificationContent,
                publishedEvent.Chapter.Id)));

        Task<ModerationAnalysis> analysisTask;

        string? thumbnailUrl = string.IsNullOrEmpty(publishedVersion.Thumbnail)
            ? null
            : await _imageStore.GetUrl($"thumbnails/{publishedVersion.Thumbnail}", DateTime.UtcNow.AddMinutes(2));

        if (volume.Series.Type == SeriesType.Comic)
        {
            List<string> images = await _comicChapterService.GetImageUrls(publishedVersion?.Content ?? string.Empty);

            if (thumbnailUrl is not null)
            {
                images.Add(thumbnailUrl);
            }

            analysisTask = _moderationService.ModerateComicChapter(images);
        }
        else
        {
            List<string> images = await _novelChapterService.GetImageUrls(publishedVersion?.Content ?? string.Empty);

            if (thumbnailUrl is not null)
            {
                images.Add(thumbnailUrl);
            }

            string text = _novelChapterService.GetTextContent(publishedVersion?.Content ?? string.Empty);

            analysisTask = _moderationService.ModerateNovelChapter(images, text);
        }

        ModerationAnalysis analysis = await analysisTask;

        if (!analysis.Flagged)
        {
            return;
        }

        Report report = Report.CreateNewSystemReport(ReportType.Content, publishedEvent.Chapter.Id);

        report.AddAnalytics(analysis);

        _context.Reports.Add(report);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
