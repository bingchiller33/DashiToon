namespace DashiToon.Api.Application.Moderation.Model;

public sealed record ReportVm(
    string? ReportedByUsername,
    string Reason,
    string ReportedAt,
    bool? Flagged,
    Dictionary<string, float> FlaggedCategories
);
