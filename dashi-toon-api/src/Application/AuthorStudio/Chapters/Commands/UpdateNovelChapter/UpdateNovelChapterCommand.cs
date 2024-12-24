using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Common.Services;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using FluentValidation.Results;
using ValidationException = DashiToon.Api.Application.Common.Exceptions.ValidationException;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.Commands.UpdateNovelChapter;

[Authorize]
[Restrict(Require = Restrictions.NotRestricted)]
public sealed record UpdateNovelChapterCommand(
    int SeriesId,
    int VolumeId,
    int ChapterId,
    string Title,
    string? Thumbnail,
    string Content,
    string? Note,
    bool IsAutoSave = false
) : IRequest;

public sealed class UpdateNovelChapterCommandHandler : IRequestHandler<UpdateNovelChapterCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly NovelChapterService _novelChapterService;
    private readonly IUser _user;

    public UpdateNovelChapterCommandHandler(
        IApplicationDbContext dbContext,
        NovelChapterService novelChapterService,
        IUser user)
    {
        _dbContext = dbContext;
        _novelChapterService = novelChapterService;
        _user = user;
    }

    public async Task Handle(UpdateNovelChapterCommand request, CancellationToken cancellationToken)
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

        if (series.Type != SeriesType.Novel)
        {
            throw new ValidationException([
                new ValidationFailure(nameof(series), "Not of novel type ")
            ]);
        }

        if (!series.Volumes.Any())
        {
            throw new NotFoundException(request.VolumeId.ToString(), nameof(Volume));
        }

        Chapter? chapter = _dbContext.Chapters
            .FirstOrDefault(c => c.Id == request.ChapterId && c.VolumeId == request.VolumeId);

        if (chapter is null)
        {
            throw new NotFoundException(request.ChapterId.ToString(), nameof(Chapter));
        }

        await _novelChapterService.UpdateChapter(
            chapter,
            request.Title,
            request.Thumbnail,
            request.Content,
            request.Note,
            request.IsAutoSave);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
