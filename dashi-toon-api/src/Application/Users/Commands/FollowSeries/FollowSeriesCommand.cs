using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using DashiToon.Api.Domain.Exceptions;

namespace DashiToon.Api.Application.Users.Commands.FollowSeries;

[Authorize]
public sealed record FollowSeriesCommand(int SeriesId) : IRequest;

public class FollowSeriesCommandHandler : IRequestHandler<FollowSeriesCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;

    public FollowSeriesCommandHandler(IApplicationDbContext dbContext, IUser user)
    {
        _dbContext = dbContext;
        _user = user;
    }

    public async Task Handle(FollowSeriesCommand request, CancellationToken cancellationToken)
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

        if (_dbContext.Follows.Any(f => f.UserId == _user.Id! && f.SeriesId == request.SeriesId))
        {
            throw new AlreadyFollowedException();
        }

        Follow? follow = Follow.CreateNew(request.SeriesId, _user.Id!);

        _dbContext.Follows.Add(follow);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
