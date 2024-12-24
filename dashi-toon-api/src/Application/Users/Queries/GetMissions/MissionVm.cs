namespace DashiToon.Api.Application.Users.Queries.GetMissions;

public sealed record MissionVm(Guid MissionId, int Amount, int ReadCount, bool IsCompleted, bool IsCompletable);
