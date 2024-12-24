using DashiToon.Api.Application.AuthorStudio.Revenue.Models;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.AuthorStudio.Revenue.Queries.GetRevenue;

[Authorize]
public sealed record GetRevenueQuery : IRequest<RevenueVm>;

public sealed class GetRevenueQueryHandler : IRequestHandler<GetRevenueQuery, RevenueVm>
{
    private readonly IUserRepository _userRepository;
    private readonly IUser _user;

    public GetRevenueQueryHandler(IUserRepository userRepository, IUser user)
    {
        _userRepository = userRepository;
        _user = user;
    }

    public async Task<RevenueVm> Handle(GetRevenueQuery request, CancellationToken cancellationToken)
    {
        IDomainUser? user = await _userRepository.GetById(_user.Id!);

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }

        IReadOnlyList<RevenueTransaction>? revenueTransactions = user.RevenueTransactions;

        decimal totalRevenue = revenueTransactions
            .Where(rt => rt.TransactionType != RevenueTransactionType.Withdraw)
            .Sum(t => t.Amount);

        List<RevenueBreakdown>? revenueBreakdown = revenueTransactions
            .GroupBy(t => new { t.Timestamp.Year, t.Timestamp.Month })
            .OrderBy(x => x.Key.Year)
            .ThenBy(x => x.Key.Month)
            .Select(g => new RevenueBreakdown
                (
                    $"{g.Key.Month}-{g.Key.Year}",
                    g.Where(t => t.TransactionType == RevenueTransactionType.Earn)
                        .Sum(t => t.Amount),
                    g.Where(t => t.TransactionType == RevenueTransactionType.Withdraw)
                        .Sum(t => t.Amount) * -1
                )
            ).ToList();

        List<RevenueBreakdown>? latestTwoMonthRevenues = revenueBreakdown
            .SkipLast(1)
            .TakeLast(2)
            .ToList();

        decimal momGrowth = 0M;

        if (latestTwoMonthRevenues.Count == 2)
        {
            momGrowth = Math.Round(
                (latestTwoMonthRevenues[1].Revenue - latestTwoMonthRevenues[0].Revenue) * 100 /
                latestTwoMonthRevenues[0].Revenue,
                2,
                MidpointRounding.ToNegativeInfinity);
        }

        return new RevenueVm(
            user.Revenue,
            totalRevenue,
            momGrowth,
            revenueBreakdown
        );
    }
}
