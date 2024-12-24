using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.Commands.ReorderChapter;

[Authorize]
[Restrict(Require = Restrictions.NotRestricted)]
public sealed record ReorderChapterCommand(int SeriesId, int VolumeId, int ChapterId, int PreviousChapterId)
    : IRequest;

public sealed class ReorderChapterCommandHandler : IRequestHandler<ReorderChapterCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;

    public ReorderChapterCommandHandler(IApplicationDbContext dbContext, IUser user)
    {
        _dbContext = dbContext;
        _user = user;
    }

    public async Task Handle(ReorderChapterCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Series? series = await _dbContext.Series
            .Include(s => s.Volumes.Where(v => v.Id == request.VolumeId))
            .ThenInclude(v => v.Chapters)
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

        Volume volume = series.Volumes[0];

        volume.ReorderChapter(request.ChapterId, request.PreviousChapterId);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
