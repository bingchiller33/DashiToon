using DashiToon.Api.Application.AuthorStudio.Chapters.Models;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.SeriesDetail.Queries.GetOwnedChapters;

[Authorize]
public sealed record GetOwnedChaptersQuery(int SeriesId, int VolumeId) : IRequest<List<OwnedChapterVm>>;

public sealed class GetOwnedChaptersQueryHandler : IRequestHandler<GetOwnedChaptersQuery, List<OwnedChapterVm>>
{
    private readonly IChapterRepository _chapterRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IImageStore _imageStore;
    private readonly IUser _user;

    public GetOwnedChaptersQueryHandler(
        IChapterRepository chapterRepository,
        IApplicationDbContext dbContext,
        IImageStore imageStore,
        IUser user
    )
    {
        _chapterRepository = chapterRepository;
        _dbContext = dbContext;
        _imageStore = imageStore;
        _user = user;
    }

    public async Task<List<OwnedChapterVm>> Handle(GetOwnedChaptersQuery request, CancellationToken cancellationToken)
    {
        Series? series = await _dbContext.Series
            .Include(s => s.Volumes.Where(v => v.Id == request.SeriesId))
            .FirstOrDefaultAsync(s => s.Id == request.SeriesId, cancellationToken);

        if (series is null)
        {
            throw new NotFoundException(request.SeriesId.ToString(), nameof(Series));
        }

        if (!series.Volumes.Any())
        {
            throw new NotFoundException(request.VolumeId.ToString(), nameof(Volume));
        }

        IEnumerable<int> chapterIds = await _chapterRepository.FindUserUnlockedChapterIds(_user.Id);

        IEnumerable<ChapterInfo> chapters = await _chapterRepository.FindPublishedChaptersByVolumeId(request.VolumeId);

        return chapters.Select(c => new OwnedChapterVm(
            c.ChapterId,
            c.ChapterNumber,
            c.Title,
            string.IsNullOrEmpty(c.Thumbnail)
                ? _imageStore
                    .GetUrl("thumbnails/default.png", DateTime.UtcNow.AddMinutes(2))
                    .GetAwaiter()
                    .GetResult()
                : _imageStore
                    .GetUrl($"thumbnails/{c.Thumbnail}", DateTime.UtcNow.AddMinutes(2))
                    .GetAwaiter()
                    .GetResult(),
            c.PublishedDate?.ToString("O"),
            c.KanaPrice,
            chapterIds.Contains(c.ChapterId)
        )).ToList();
    }
}
