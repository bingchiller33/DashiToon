using DashiToon.Api.Application.AuthorStudio.Chapters.Models;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.Queries.GetChapters;

[Authorize]
public sealed record GetChaptersQuery(
    int SeriesId,
    int VolumeId,
    string? Title,
    ChapterStatus? Status,
    int PageNumber = 1,
    int PageSize = 10)
    : IRequest<PaginatedList<ChapterVm>>;

public sealed class GetChaptersQueryHandler : IRequestHandler<GetChaptersQuery, PaginatedList<ChapterVm>>
{
    private readonly IChapterRepository _chapterRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IImageStore _imageStore;
    private readonly IUser _user;

    public GetChaptersQueryHandler(
        IApplicationDbContext dbContext,
        IChapterRepository chapterRepository,
        IImageStore imageStore,
        IUser user)
    {
        _dbContext = dbContext;
        _chapterRepository = chapterRepository;
        _imageStore = imageStore;
        _user = user;
    }

    public async Task<PaginatedList<ChapterVm>> Handle(GetChaptersQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.Series? series = await _dbContext.Series
            .Include(s => s.Volumes.Where(v => v.Id == request.VolumeId))
            .FirstOrDefaultAsync(s => s.Id == request.SeriesId, cancellationToken);

        if (series is null)
        {
            throw new NotFoundException(request.SeriesId.ToString(), nameof(Series));
        }

        if (series.CreatedBy != _user.Id)
        {
            throw new ForbiddenAccessException();
        }

        if (!series.Volumes.Any())
        {
            throw new NotFoundException(request.VolumeId.ToString(), nameof(Volume));
        }

        (int count, IEnumerable<ChapterInfo> chapters) = await _chapterRepository
            .FindChapters(
                request.Title,
                request.Status,
                request.VolumeId,
                request.PageNumber,
                request.PageSize
            );

        List<ChapterVm>? result = new(request.PageSize);

        foreach (ChapterInfo? chapter in chapters)
        {
            result.Add(new ChapterVm(
                chapter.ChapterId,
                chapter.ChapterNumber,
                chapter.Title,
                string.IsNullOrEmpty(chapter.Thumbnail)
                    ? await _imageStore.GetUrl("thumbnails/default.png", DateTime.UtcNow.AddMinutes(2))
                    : await _imageStore.GetUrl($"thumbnails/{chapter.Thumbnail}", DateTime.UtcNow.AddMinutes(2)),
                chapter.KanaPrice,
                chapter.Status,
                chapter.PublishedDate?.ToString("O")));
        }

        return new PaginatedList<ChapterVm>(
            result,
            count,
            request.PageNumber,
            request.PageSize);
    }
}
