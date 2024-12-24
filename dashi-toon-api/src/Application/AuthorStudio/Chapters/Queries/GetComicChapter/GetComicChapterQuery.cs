using DashiToon.Api.Application.AuthorStudio.Chapters.Models;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Common.Services;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using FluentValidation.Results;
using ValidationException = DashiToon.Api.Application.Common.Exceptions.ValidationException;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.Queries.GetComicChapter;

[Authorize]
public sealed record GetComicChapterQuery(int SeriesId, int VolumeId, int ChapterId) : IRequest<ComicChapterDetailVm>;

public sealed class GetComicChapterQueryHandler : IRequestHandler<GetComicChapterQuery, ComicChapterDetailVm>
{
    private readonly ComicChapterService _comicChapterService;
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;

    public GetComicChapterQueryHandler(
        IApplicationDbContext dbContext,
        ComicChapterService comicChapterService,
        IUser user)
    {
        _dbContext = dbContext;
        _comicChapterService = comicChapterService;
        _user = user;
    }

    public async Task<ComicChapterDetailVm> Handle(GetComicChapterQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.Series? series = await _dbContext.Series
            .Include(v => v.Volumes.Where(x => x.Id == request.VolumeId))
            .FirstOrDefaultAsync(v => v.Id == request.SeriesId, cancellationToken);

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

        Chapter? chapter = await _dbContext.Chapters
            .FirstOrDefaultAsync(c => c.Id == request.ChapterId && c.VolumeId == request.VolumeId,
                cancellationToken);

        if (chapter is null)
        {
            throw new NotFoundException(request.ChapterId.ToString(), nameof(Chapter));
        }

        ChapterVersion currentVersion = chapter.GetCurrentVersion();

        List<ImageInfo> content = await _comicChapterService.ProcessContent(currentVersion.Content);

        return new ComicChapterDetailVm(
            chapter.Id,
            chapter.CurrentVersionId,
            chapter.PublishedVersionId,
            currentVersion.Title,
            currentVersion.Thumbnail,
            content,
            currentVersion.Note,
            chapter.KanaPrice,
            currentVersion.Status,
            chapter.PublishedDate?.ToString("O") ?? null,
            currentVersion.Timestamp.ToString("O")
        );
    }
}
