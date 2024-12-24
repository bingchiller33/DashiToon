using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.Commands.BulkPublishChapter;

[Authorize]
[Restrict(Require = Restrictions.NotRestricted)]
public sealed record BulkPublishChapterCommand(int SeriesId, int VolumeId, IEnumerable<int> ChapterIds) : IRequest;

public sealed class BulkPublishChapterCommandHandler : IRequestHandler<BulkPublishChapterCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public BulkPublishChapterCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(BulkPublishChapterCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Series? series = await _context.Series
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

        List<Chapter> chapters = await _context.Chapters
            .Where(c => request.ChapterIds.Contains(c.Id) && c.VolumeId == request.VolumeId)
            .ToListAsync(cancellationToken);

        foreach (Chapter chapter in chapters)
        {
            chapter.PublishImmediately();
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
