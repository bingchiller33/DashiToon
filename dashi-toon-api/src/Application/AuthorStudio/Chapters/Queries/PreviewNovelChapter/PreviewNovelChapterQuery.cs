using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Common.Services;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using FluentValidation.Results;
using ValidationException = DashiToon.Api.Application.Common.Exceptions.ValidationException;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.Queries.PreviewNovelChapter;

[Authorize]
public sealed record PreviewNovelChapterQuery(
    int SeriesId,
    int VolumeId,
    int ChapterId,
    Guid VersionId) : IRequest<NovelChapterPreviewVm>;

public sealed class PreviewNovelChapterQueryHandler : IRequestHandler<PreviewNovelChapterQuery, NovelChapterPreviewVm>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly NovelChapterService _novelChapterService;
    private readonly IUser _user;

    public PreviewNovelChapterQueryHandler(
        IApplicationDbContext dbContext,
        IUser user,
        NovelChapterService novelChapterService)
    {
        _dbContext = dbContext;
        _user = user;
        _novelChapterService = novelChapterService;
    }

    public async Task<NovelChapterPreviewVm> Handle(PreviewNovelChapterQuery request,
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

        if (series.Type is not SeriesType.Novel)
        {
            throw new ValidationException([
                new ValidationFailure(nameof(Series), "Not of novel type")
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

        string processContent = await _novelChapterService.ProcessContent(previewVersion.Content);

        return new NovelChapterPreviewVm(
            chapter.Id,
            previewVersion.Id,
            previewVersion.VersionName,
            previewVersion.Title,
            previewVersion.Thumbnail,
            processContent,
            previewVersion.Note
        );
    }
}
