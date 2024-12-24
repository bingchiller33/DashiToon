using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Common.Services;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using FluentValidation.Results;
using ValidationException = DashiToon.Api.Application.Common.Exceptions.ValidationException;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateNovelChapter;

[Authorize]
[Restrict(Require = Restrictions.NotRestricted)]
public sealed record CreateNovelChapterCommand(
    int SeriesId,
    int VolumeId,
    string Title,
    string? Thumbnail,
    string Content,
    string? Note
) : IRequest<int>;

public sealed class CreateNovelChapterCommandHandler : IRequestHandler<CreateNovelChapterCommand, int>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly NovelChapterService _novelChapterService;
    private readonly IUser _user;

    public CreateNovelChapterCommandHandler(
        IApplicationDbContext dbContext,
        NovelChapterService novelChapterService,
        IUser user)
    {
        _dbContext = dbContext;
        _novelChapterService = novelChapterService;
        _user = user;
    }

    public async Task<int> Handle(CreateNovelChapterCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Series? series =
            await _dbContext.Series.FirstOrDefaultAsync(s => s.Id == request.SeriesId, cancellationToken);

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
                new ValidationFailure(nameof(series), "Not of novel type")
            ]);
        }

        Volume? volume = await _dbContext.Volumes
            .Include(v => v.Chapters)
            .FirstOrDefaultAsync(v => v.SeriesId == request.SeriesId && v.Id == request.VolumeId,
                cancellationToken);

        if (volume is null)
        {
            throw new NotFoundException(request.VolumeId.ToString(), nameof(Volume));
        }

        Chapter chapter = await _novelChapterService.CreateChapter(
            request.Title,
            request.Thumbnail,
            request.Content,
            request.Note);

        volume.AddNewChapter(chapter);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return chapter.Id;
    }
}
