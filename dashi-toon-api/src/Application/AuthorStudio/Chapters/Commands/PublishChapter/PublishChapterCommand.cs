using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.Commands.PublishChapter;

[Authorize]
[Restrict(Require = Restrictions.NotRestricted)]
public sealed record PublishChapterCommand(
    int SeriesId,
    int VolumeId,
    int ChapterId,
    DateTimeOffset? PublishDate = null) : IRequest;

public sealed class PublishChapterCommandHandler : IRequestHandler<PublishChapterCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public PublishChapterCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(PublishChapterCommand request, CancellationToken cancellationToken)
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

        if (!series.Volumes.Any())
        {
            throw new NotFoundException(request.VolumeId.ToString(), nameof(Volume));
        }

        Chapter? chapter = await _context.Chapters.FirstOrDefaultAsync(
            c => c.Id == request.ChapterId && c.VolumeId == request.VolumeId,
            cancellationToken);

        if (chapter is null)
        {
            throw new NotFoundException(request.ChapterId.ToString(), nameof(Chapter));
        }

        if (request.PublishDate.HasValue)
        {
            chapter.SchedulePublish(request.PublishDate.Value);
        }
        else
        {
            chapter.PublishImmediately();
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
