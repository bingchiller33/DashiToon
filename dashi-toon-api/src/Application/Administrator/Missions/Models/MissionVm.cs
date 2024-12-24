namespace DashiToon.Api.Application.Administrator.Missions.Models;

public sealed record MissionVm(
    Guid Id,
    int ReadCount,
    int Reward,
    bool IsActive
);
