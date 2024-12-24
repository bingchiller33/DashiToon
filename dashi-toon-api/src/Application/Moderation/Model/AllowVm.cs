namespace DashiToon.Api.Application.Moderation.Model;

public sealed record AllowVm(bool IsAllowed, string? NotAllowedUntil = null);
