using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.Commands.RestoreChapter;

[Authorize]
[Restrict(Require = Restrictions.NotRestricted)]
public sealed record RestoreChapterCommand(int SeriesId, int VolumeId, int ChapterId, Guid VersionId) : IRequest;

public sealed class RestoreChapterCommandHandler : IRequestHandler<RestoreChapterCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;

    public RestoreChapterCommandHandler(IApplicationDbContext dbContext, IUser user)
    {
        _dbContext = dbContext;
        _user = user;
    }

    public async Task Handle(RestoreChapterCommand request, CancellationToken cancellationToken)
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

        if (!series.Volumes.Any())
        {
            throw new NotFoundException(request.VolumeId.ToString(), nameof(Volume));
        }

        Chapter? chapter = await _dbContext.Chapters.FirstOrDefaultAsync(
            c => c.Id == request.ChapterId && c.VolumeId == request.VolumeId,
            cancellationToken);

        if (chapter is null)
        {
            throw new NotFoundException(request.ChapterId.ToString(), nameof(Chapter));
        }

        chapter.RestoreVersion(request.VersionId);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
