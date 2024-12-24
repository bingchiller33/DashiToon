using DashiToon.Api.Domain.Enums;
using DashiToon.Api.Domain.ValueObjects;

namespace DashiToon.Api.Application.Users.Queries.GetPaymentHistory;

public record struct PaymentVm(
    string Id,
    string Detail,
    Price? Price,
    OrderStatus Status,
    string CompletedAt
);
