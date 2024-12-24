using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Common.Security;

namespace DashiToon.Api.Application.Users.Queries.GetFollowedSeries;

[Authorize]
public sealed record GetFollowedSeriesQuery(
    bool? HasRead,
    string SortBy = "LastRead",
    string SortOrder = "Desc",
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PaginatedList<FollowedSeriesVm>>;

public sealed class GetFollowedSeriesQueryHandler
    : IRequestHandler<GetFollowedSeriesQuery, PaginatedList<FollowedSeriesVm>>
{
    private readonly IFollowRepository _followRepository;
    private readonly IImageStore _imageStore;
    private readonly IUser _user;

    public GetFollowedSeriesQueryHandler(IFollowRepository followRepository, IImageStore imageStore, IUser user)
    {
        _followRepository = followRepository;
        _imageStore = imageStore;
        _user = user;
    }

    public async Task<PaginatedList<FollowedSeriesVm>> Handle(
        GetFollowedSeriesQuery request,
        CancellationToken cancellationToken)
    {
        (int count, IEnumerable<FollowedSeriesDto> series) = await _followRepository.GetFollowedSeries(
            _user.Id!,
            request.HasRead,
            request.SortBy,
            request.SortOrder,
            request.PageNumber,
            request.PageSize);

        List<FollowedSeriesVm> result = new(request.PageSize);

        foreach (FollowedSeriesDto s in series)
        {
            result.Add(new FollowedSeriesVm(
                string.IsNullOrEmpty(s.Thumbnail)
                    ? await _imageStore.GetUrl("thumbnails/default.png", DateTime.UtcNow.AddMinutes(2))
                    : await _imageStore.GetUrl($"thumbnails/{s.Thumbnail}", DateTime.UtcNow.AddMinutes(2)),
                s.Title,
                s.Type,
                s.Status,
                s.SeriesId,
                s.VolumeId,
                s.LatestChapterId,
                s.Progress,
                s.TotalChapters,
                s.IsNotified
            ));
        }

        return new PaginatedList<FollowedSeriesVm>(
            result,
            count,
            request.PageNumber,
            request.PageSize
        );
    }
}
