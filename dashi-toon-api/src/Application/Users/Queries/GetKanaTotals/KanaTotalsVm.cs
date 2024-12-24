using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.Users.Queries.GetKanaTotals;

public sealed record KanaTotalsVm(
    KanaTotal[] Totals
);

public sealed record KanaTotal(
    KanaType KanaType,
    int Amount);
