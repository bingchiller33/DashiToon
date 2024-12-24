using DashiToon.Api.Application.AuthorStudio.Revenue.Models;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.AuthorStudio.Revenue.Queries.GetRevenueTransactions;

[Authorize]
public sealed record GetRevenueTransactionsQuery(
    RevenueTransactionType Type,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PaginatedList<RevenueTransactionVm>>;

public sealed class GetRevenueTransactionsQueryHandler
    : IRequestHandler<GetRevenueTransactionsQuery, PaginatedList<RevenueTransactionVm>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUser _user;

    public GetRevenueTransactionsQueryHandler(IUserRepository userRepository, IUser user)
    {
        _userRepository = userRepository;
        _user = user;
    }

    public async Task<PaginatedList<RevenueTransactionVm>> Handle(
        GetRevenueTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        IDomainUser? author = await _userRepository.GetById(_user.Id!);

        if (author is null)
        {
            throw new UnauthorizedAccessException();
        }

        List<RevenueTransactionVm>? transactions = author.RevenueTransactions
            .Where(rt => rt.TransactionType == request.Type)
            .Select(rt => new RevenueTransactionVm(
                rt.Amount,
                rt.Type,
                rt.TransactionType,
                rt.Reason,
                rt.Timestamp.ToString("O")
            ))
            .ToList();

        return new PaginatedList<RevenueTransactionVm>(
            transactions
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList(),
            transactions.Count,
            request.PageNumber,
            request.PageSize
        );
    }
}
