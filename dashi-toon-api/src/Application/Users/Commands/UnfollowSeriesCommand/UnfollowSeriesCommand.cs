using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.Users.Commands.UnfollowSeriesCommand;

[Authorize]
public sealed record UnfollowSeriesCommand(int SeriesId) : IRequest;

public class UnfollowSeriesCommandHandler : IRequestHandler<UnfollowSeriesCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;

    public UnfollowSeriesCommandHandler(IApplicationDbContext dbContext, IUser user)
    {
        _dbContext = dbContext;
        _user = user;
    }

    public async Task Handle(UnfollowSeriesCommand request, CancellationToken cancellationToken)
    {
        Series? series = await _dbContext.Series.FirstOrDefaultAsync(s =>
                s.Id == request.SeriesId
                && s.Status != SeriesStatus.Draft
                && s.Status != SeriesStatus.Hiatus
                && s.Status != SeriesStatus.Trashed,
            cancellationToken);

        if (series is null)
        {
            throw new NotFoundException(request.SeriesId.ToString(), nameof(Series));
        }

        Follow? follow = await _dbContext.Follows.FirstOrDefaultAsync(
            f => f.SeriesId == request.SeriesId && f.UserId == _user.Id,
            cancellationToken);

        if (follow is not null)
        {
            _dbContext.Follows.Remove(follow);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
