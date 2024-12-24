using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.AuthorStudio.Revenue.Models;

public sealed record RevenueTransactionVm(
    decimal Amount,
    RevenueType RevenueType,
    RevenueTransactionType TransactionType,
    string Reason,
    string Timestamp
);
