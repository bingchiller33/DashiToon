using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.Users.Queries.GetKanaTransactions;

[Authorize]
public sealed record GetKanaTransactionsQuery(
    string Type,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PaginatedList<KanaTransactionVm>>;

public sealed class GetKanaTransactionsQueryHandler
    : IRequestHandler<GetKanaTransactionsQuery, PaginatedList<KanaTransactionVm>>
{
    private readonly IUser _user;
    private readonly IUserRepository _userRepository;

    public GetKanaTransactionsQueryHandler(IUser user, IUserRepository userRepository)
    {
        _user = user;
        _userRepository = userRepository;
    }

    public async Task<PaginatedList<KanaTransactionVm>> Handle(
        GetKanaTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        IDomainUser? user = await _userRepository.GetById(_user.Id ?? string.Empty);

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }

        if (request.Type == "SPENT")
        {
            List<KanaTransactionVm> result = user.Ledgers
                .Where(transaction =>
                    transaction.Type == TransactionType.Spend).Select(
                    transaction =>
                        new KanaTransactionVm(
                            transaction.Amount,
                            transaction.Currency,
                            transaction.Type,
                            transaction.Reason,
                            transaction.Timestamp.ToString("O")
                        )).ToList();

            return new PaginatedList<KanaTransactionVm>(
                result
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList(),
                result.Count,
                request.PageNumber,
                request.PageSize);
        }
        else
        {
            List<KanaTransactionVm> result = user.Ledgers
                .Where(transaction =>
                    transaction.Type != TransactionType.Spend).Select(
                    transaction =>
                        new KanaTransactionVm(
                            transaction.Amount,
                            transaction.Currency,
                            transaction.Type,
                            transaction.Reason,
                            transaction.Timestamp.ToString("O")
                        )).ToList();

            return new PaginatedList<KanaTransactionVm>(
                result
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList(),
                result.Count,
                request.PageNumber,
                request.PageSize);
        }
    }
}
