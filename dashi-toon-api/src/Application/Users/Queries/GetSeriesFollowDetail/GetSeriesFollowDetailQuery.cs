using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;

namespace DashiToon.Api.Application.Users.Queries.GetSeriesFollowDetail;

[Authorize]
public sealed record GetSeriesFollowDetailQuery(int SeriesId) : IRequest<SeriesFollowDetailVm?>;

public sealed class GetSeriesFollowDetailQueryHandler
    : IRequestHandler<GetSeriesFollowDetailQuery, SeriesFollowDetailVm?>
{
    private readonly IFollowRepository _followRepository;
    private readonly IUser _user;

    public GetSeriesFollowDetailQueryHandler(IFollowRepository followRepository, IUser user)
    {
        _followRepository = followRepository;
        _user = user;
    }

    public async Task<SeriesFollowDetailVm?> Handle(
        GetSeriesFollowDetailQuery request,
        CancellationToken cancellationToken)
    {
        FollowDetailDto? follow = await _followRepository.GetFollowedSeriesById(_user.Id!, request.SeriesId);

        if (follow is null)
        {
            return new SeriesFollowDetailVm(false, null);
        }

        return new SeriesFollowDetailVm(
            true,
            new FollowDetailVm(
                follow.SeriesId,
                follow.VolumeId,
                follow.LatestChapterId,
                follow.Progress,
                follow.TotalChapters,
                follow.IsNotified
            )
        );
    }
}
