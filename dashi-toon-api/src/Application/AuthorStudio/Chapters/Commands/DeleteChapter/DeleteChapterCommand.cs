using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.Commands.DeleteChapter;

[Authorize]
[Restrict(Require = Restrictions.NotRestricted)]
public sealed record DeleteChapterCommand(int SeriesId, int VolumeId, int ChapterId) : IRequest;

public sealed class DeleteChapterCommandHandler : IRequestHandler<DeleteChapterCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public DeleteChapterCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(DeleteChapterCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Series? series = await _context.Series
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

        series.Volumes[0].RemoveChapter(request.ChapterId);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
