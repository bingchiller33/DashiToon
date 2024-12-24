using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.Users.Queries.GetSeriesFollowStatus;

[Authorize]
public sealed record GetSeriesFollowStatusQuery(int SeriesId) : IRequest<bool>;

public sealed class GetSeriesFollowStatusQueryHandler : IRequestHandler<GetSeriesFollowStatusQuery, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetSeriesFollowStatusQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<bool> Handle(GetSeriesFollowStatusQuery request, CancellationToken cancellationToken)
    {
        Series? series = await _context.Series.FirstOrDefaultAsync(s =>
                s.Id == request.SeriesId
                && s.Status != SeriesStatus.Draft
                && s.Status != SeriesStatus.Hiatus
                && s.Status != SeriesStatus.Trashed,
            cancellationToken);

        if (series is null)
        {
            throw new NotFoundException(request.SeriesId.ToString(), nameof(Series));
        }

        Follow? follow = await _context.Follows.FirstOrDefaultAsync(
            f => f.UserId == _user.Id && f.SeriesId == request.SeriesId,
            cancellationToken);

        return follow is not null;
    }
}
