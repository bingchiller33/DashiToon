using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Common.Services;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using FluentValidation.Results;
using ValidationException = DashiToon.Api.Application.Common.Exceptions.ValidationException;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.Queries.GetNovelChapter;

[Authorize]
public sealed record GetNovelChapterQuery(int SeriesId, int VolumeId, int ChapterId) : IRequest<NovelChapterDetailVm>;

public sealed class GetChapterQueryHandler : IRequestHandler<GetNovelChapterQuery, NovelChapterDetailVm>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly NovelChapterService _novelChapterService;
    private readonly IUser _user;

    public GetChapterQueryHandler(
        IApplicationDbContext dbContext,
        NovelChapterService novelChapterService,
        IUser user)
    {
        _dbContext = dbContext;
        _novelChapterService = novelChapterService;
        _user = user;
    }

    public async Task<NovelChapterDetailVm> Handle(GetNovelChapterQuery request, CancellationToken cancellationToken)
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

        ChapterVersion currentVersion = chapter.GetCurrentVersion();

        string processContent = await _novelChapterService.ProcessContent(currentVersion.Content);

        return new NovelChapterDetailVm(
            chapter.Id,
            chapter.CurrentVersionId,
            chapter.PublishedVersionId,
            currentVersion.Title,
            currentVersion.Thumbnail,
            processContent,
            currentVersion.Note,
            chapter.KanaPrice,
            currentVersion.Status,
            chapter.PublishedDate?.ToString("O") ?? null,
            currentVersion.Timestamp.ToString("O")
        );
    }
}
