using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.Users.Queries.GetKanaTransactions;

public sealed record KanaTransactionVm(
    int Amount,
    KanaType Currency,
    TransactionType Type,
    string Reason,
    string Time
);
