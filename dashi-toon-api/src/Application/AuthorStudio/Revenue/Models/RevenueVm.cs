namespace DashiToon.Api.Application.AuthorStudio.Revenue.Models;

public sealed record RevenueVm(
    decimal Balance,
    decimal TotalRevenue,
    decimal MomGrowth,
    List<RevenueBreakdown> Data
);

public sealed record RevenueBreakdown(
    string Month,
    decimal Revenue,
    decimal Withdrawal
);
