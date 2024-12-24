using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.Commands.BulkDeleteChapter;

[Authorize]
[Restrict(Require = Restrictions.NotRestricted)]
public sealed record BulkDeleteChapterCommand(int Id, int VolumeId, IEnumerable<int> ChapterIds) : IRequest;

public sealed class BulkDeleteChapterCommandHandler : IRequestHandler<BulkDeleteChapterCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public BulkDeleteChapterCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(BulkDeleteChapterCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Series? series = await _context.Series
            .Include(s => s.Volumes.Where(v => v.Id == request.VolumeId))
            .ThenInclude(v => v.Chapters)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (series is null)
        {
            throw new NotFoundException(request.Id.ToString(), nameof(Series));
        }

        if (series.CreatedBy != _user.Id)
        {
            throw new ForbiddenAccessException();
        }

        if (!series.Volumes.Any())
        {
            throw new NotFoundException(request.VolumeId.ToString(), nameof(Volume));
        }

        Volume volume = series.Volumes[0];

        foreach (int id in request.ChapterIds)
        {
            volume.RemoveChapter(id);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
