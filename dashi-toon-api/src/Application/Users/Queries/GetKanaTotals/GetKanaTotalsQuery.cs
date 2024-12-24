using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.Users.Queries.GetKanaTotals;

[Authorize]
public class GetKanaTotalsQuery : IRequest<KanaTotalsVm>;

public sealed class GetKanaTotalsQueryHandler : IRequestHandler<GetKanaTotalsQuery, KanaTotalsVm>
{
    private readonly IUser _user;
    private readonly IUserRepository _userRepository;

    public GetKanaTotalsQueryHandler(IUser user, IUserRepository userRepository)
    {
        _user = user;
        _userRepository = userRepository;
    }

    public async Task<KanaTotalsVm> Handle(GetKanaTotalsQuery request, CancellationToken cancellationToken)
    {
        IDomainUser? user = await _userRepository.GetById(_user.Id ?? string.Empty);

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }

        return new KanaTotalsVm([
            new KanaTotal(KanaType.Coin, user.KanaCoin), new KanaTotal(KanaType.Gold, user.KanaGold)
        ]);
    }
}
