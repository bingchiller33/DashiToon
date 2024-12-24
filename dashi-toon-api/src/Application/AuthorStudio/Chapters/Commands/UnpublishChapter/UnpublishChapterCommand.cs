using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.Commands.UnpublishChapter;

[Authorize]
[Restrict(Require = Restrictions.NotRestricted)]
public sealed record UnpublishChapterCommand(
    int SeriesId,
    int VolumeId,
    int ChapterId) : IRequest;

public sealed class UnpublishChapterCommandHandler : IRequestHandler<UnpublishChapterCommand>
{
    private readonly IUser _user;
    private readonly IApplicationDbContext _context;

    public UnpublishChapterCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(UnpublishChapterCommand request, CancellationToken cancellationToken)
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

        Chapter? chapter = await _context.Chapters.FirstOrDefaultAsync(
            c => c.Id == request.ChapterId && c.VolumeId == request.VolumeId,
            cancellationToken);

        if (chapter is null)
        {
            throw new NotFoundException(request.ChapterId.ToString(), nameof(Chapter));
        }

        chapter.Unpublish();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
