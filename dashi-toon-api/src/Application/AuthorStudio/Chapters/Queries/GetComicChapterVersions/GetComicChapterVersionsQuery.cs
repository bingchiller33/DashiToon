using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Common.Services;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using FluentValidation.Results;
using ValidationException = DashiToon.Api.Application.Common.Exceptions.ValidationException;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.Queries.GetComicChapterVersions;

[Authorize]
public sealed record GetComicChapterVersionsQuery(
    int SeriesId,
    int VolumeId,
    int ChapterId,
    string? VersionName,
    bool IncludeAutoSave,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int PageNumber = 1,
    int PageSize = 10)
    : IRequest<PaginatedList<ComicChapterVersionVm>>;

public sealed record GetComicChapterVersionsQueryHandler
    : IRequestHandler<GetComicChapterVersionsQuery, PaginatedList<ComicChapterVersionVm>>
{
    private readonly IChapterRepository _chapterRepository;
    private readonly ComicChapterService _comicChapterService;
    private readonly IApplicationDbContext _context;
    private readonly IImageStore _imageStore;
    private readonly IUser _user;

    public GetComicChapterVersionsQueryHandler(
        IUser user,
        IApplicationDbContext context,
        IChapterRepository chapterRepository,
        IImageStore imageStore,
        ComicChapterService comicChapterService)
    {
        _user = user;
        _context = context;
        _imageStore = imageStore;
        _chapterRepository = chapterRepository;
        _comicChapterService = comicChapterService;
    }

    public async Task<PaginatedList<ComicChapterVersionVm>> Handle(
        GetComicChapterVersionsQuery request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Series? series = await _context.Series
            .Include(s => s.Volumes.Where(v => v.Id == request.VolumeId))
            .ThenInclude(v => v.Chapters.Where(c => c.Id == request.ChapterId))
            .FirstOrDefaultAsync(s => s.Id == request.SeriesId, cancellationToken);

        if (series is null)
        {
            throw new NotFoundException(request.SeriesId.ToString(), nameof(Series));
        }

        if (series.CreatedBy != _user.Id)
        {
            throw new ForbiddenAccessException();
        }

        if (series.Type is not SeriesType.Comic)
        {
            throw new ValidationException([
                new ValidationFailure(nameof(series), "Not of comic type")
            ]);
        }

        if (!series.Volumes.Any())
        {
            throw new NotFoundException(request.VolumeId.ToString(), nameof(Volume));
        }

        if (!series.Volumes[0].Chapters.Any())
        {
            throw new NotFoundException(request.ChapterId.ToString(), nameof(Chapter));
        }

        Chapter chapter = series.Volumes[0].Chapters[0];

        (int count, IEnumerable<ChapterVersion> versions) = await _chapterRepository.FindChapterVersions(
            request.ChapterId,
            request.VersionName ?? string.Empty,
            request.From,
            request.To,
            request.IncludeAutoSave,
            request.PageNumber,
            request.PageSize);

        List<ComicChapterVersionVm> result = new(request.PageSize);

        foreach (ChapterVersion version in versions)
        {
            result.Add(new ComicChapterVersionVm(
                version.Id,
                version.VersionName,
                version.Id == chapter.CurrentVersionId,
                version.Status == ChapterStatus.Published,
                version.IsAutoSave,
                version.Title,
                string.IsNullOrEmpty(series.Thumbnail)
                    ? await _imageStore.GetUrl("thumbnails/default.png", DateTime.UtcNow.AddMinutes(2))
                    : await _imageStore.GetUrl($"thumbnails/{series.Thumbnail}", DateTime.UtcNow.AddMinutes(2)),
                await _comicChapterService.ProcessContent(version.Content),
                version.Note,
                version.Timestamp.ToString("O")
            ));
        }

        return new PaginatedList<ComicChapterVersionVm>(
            result,
            count,
            request.PageNumber,
            request.PageSize);
    }
}
