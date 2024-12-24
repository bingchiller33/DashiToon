using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Users.Queries.GetUserMetadatas;

[Authorize]
public sealed record GetUserMetadataQuery : IRequest<UserMetadata>;

public sealed class GetUserMetadataQueryHandler : IRequestHandler<GetUserMetadataQuery, UserMetadata>
{
    private readonly IUser _user;
    private readonly IUserRepository _userRepository;

    public GetUserMetadataQueryHandler(IUser user, IUserRepository userRepository)
    {
        _user = user;
        _userRepository = userRepository;
    }

    public async Task<UserMetadata> Handle(GetUserMetadataQuery request, CancellationToken cancellationToken)
    {
        IDomainUser? user = await _userRepository.GetById(_user.Id ?? string.Empty);

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }

        return new UserMetadata(user.IsCheckedIn(), user.CurrentDayReadCount());
    }
}
