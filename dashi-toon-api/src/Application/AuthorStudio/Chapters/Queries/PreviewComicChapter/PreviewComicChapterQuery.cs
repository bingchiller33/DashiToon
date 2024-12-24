using DashiToon.Api.Application.AuthorStudio.Chapters.Models;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Common.Services;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using FluentValidation.Results;
using ValidationException = DashiToon.Api.Application.Common.Exceptions.ValidationException;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.Queries.PreviewComicChapter;

[Authorize]
public sealed record PreviewComicChapterQuery(
    int SeriesId,
    int VolumeId,
    int ChapterId,
    Guid VersionId) : IRequest<ComicChapterPreviewVm>;

public sealed class PreviewComicChapterQueryHandler : IRequestHandler<PreviewComicChapterQuery, ComicChapterPreviewVm>
{
    private readonly ComicChapterService _comicChapterService;
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;

    public PreviewComicChapterQueryHandler(
        IApplicationDbContext dbContext,
        ComicChapterService comicChapterService,
        IUser user)
    {
        _dbContext = dbContext;
        _comicChapterService = comicChapterService;
        _user = user;
    }


    public async Task<ComicChapterPreviewVm> Handle(PreviewComicChapterQuery request,
        CancellationToken cancellationToken)
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
                new ValidationFailure(nameof(Series), "Not of comic type")
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

        ChapterVersion? previewVersion = chapter.Versions.FirstOrDefault(v => v.Id == request.VersionId);

        if (previewVersion is null)
        {
            throw new NotFoundException(request.VersionId.ToString(), nameof(Version));
        }

        List<ImageInfo> content = await _comicChapterService.ProcessContent(previewVersion.Content);

        return new ComicChapterPreviewVm(
            chapter.Id,
            previewVersion.Id,
            previewVersion.VersionName,
            previewVersion.Title,
            previewVersion.Thumbnail,
            content,
            previewVersion.Note
        );
    }
}
