using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Common.Services;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using FluentValidation.Results;
using ValidationException = DashiToon.Api.Application.Common.Exceptions.ValidationException;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.Commands.UpdateComicChapter;

[Authorize]
[Restrict(Require = Restrictions.NotRestricted)]
public sealed record UpdateComicChapterCommand(
    int SeriesId,
    int VolumeId,
    int ChapterId,
    string Title,
    string? Thumbnail,
    List<string> Content,
    string? Note,
    bool IsAutoSave = false
) : IRequest;

public sealed record UpdateComicChapterCommandHandler : IRequestHandler<UpdateComicChapterCommand>
{
    private readonly ComicChapterService _comicChapterService;
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;

    public UpdateComicChapterCommandHandler(
        IApplicationDbContext dbContext,
        ComicChapterService comicChapterService,
        IUser user)
    {
        _dbContext = dbContext;
        _comicChapterService = comicChapterService;
        _user = user;
    }

    public async Task Handle(UpdateComicChapterCommand request, CancellationToken cancellationToken)
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

        if (series.Type != SeriesType.Comic)
        {
            throw new ValidationException([
                new ValidationFailure(nameof(series), "Not of comic type")
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

        await _comicChapterService.UpdateChapter(
            chapter,
            request.Title,
            request.Thumbnail,
            request.Content,
            request.Note,
            request.IsAutoSave);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
