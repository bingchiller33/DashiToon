using DashiToon.Api.Domain.ValueObjects;

namespace DashiToon.Api.Application.AuthorStudio.DashiFans.Queries.GetDashiFans;

public sealed record DashiFanVm(
    Guid Id,
    string Name,
    Price Price,
    string Description,
    int Perks,
    bool IsActive,
    string LastModified
);
