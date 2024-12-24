using DashiToon.Api.Application.AuthorStudio.Chapters.Models;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.SeriesDetail.Queries.GetSeriesChapters;

public sealed record GetSeriesChaptersQuery(int SeriesId, int VolumeId) : IRequest<List<SeriesChapterVm>>;

public sealed class GetSeriesChaptersQueryHandler : IRequestHandler<GetSeriesChaptersQuery, List<SeriesChapterVm>>
{
    private readonly IChapterRepository _chapterRepository;
    private readonly IImageStore _imageStore;
    private readonly ISeriesRepository _seriesRepository;
    private readonly IVolumeRepository _volumeRepository;

    public GetSeriesChaptersQueryHandler(
        ISeriesRepository seriesRepository,
        IVolumeRepository volumeRepository,
        IChapterRepository chapterRepository,
        IImageStore imageStore)
    {
        _seriesRepository = seriesRepository;
        _volumeRepository = volumeRepository;
        _chapterRepository = chapterRepository;
        _imageStore = imageStore;
    }

    public async Task<List<SeriesChapterVm>> Handle(GetSeriesChaptersQuery request, CancellationToken cancellationToken)
    {
        Series? series = await _seriesRepository.FindSeriesById(request.SeriesId);

        if (series is null || series.Status == SeriesStatus.Hiatus || series.Status == SeriesStatus.Trashed ||
            series.Status == SeriesStatus.Draft)
        {
            return [];
        }

        IEnumerable<Volume> volumes = await _volumeRepository.FindVolumesBySeriesId(request.SeriesId);

        if (!volumes.Select(v => v.Id).Contains(request.VolumeId))
        {
            return [];
        }

        IEnumerable<ChapterInfo> chapters = (await _chapterRepository.FindPublishedChaptersByVolumeId(request.VolumeId))
            .ToList();

        List<SeriesChapterVm>? result = new(chapters.Count());

        foreach (ChapterInfo c in chapters)
        {
            result.Add(new SeriesChapterVm(
                c.ChapterId,
                c.ChapterNumber,
                c.Title,
                string.IsNullOrEmpty(c.Thumbnail)
                    ? await _imageStore.GetUrl("thumbnails/default.png", DateTime.UtcNow.AddMinutes(2))
                    : await _imageStore.GetUrl($"thumbnails/{c.Thumbnail}", DateTime.UtcNow.AddMinutes(2)),
                c.PublishedDate?.ToString("O"),
                c.PublishedDate.HasValue && c.PublishedDate > DateTimeOffset.UtcNow,
                c.KanaPrice));
        }

        return result;
    }
}
