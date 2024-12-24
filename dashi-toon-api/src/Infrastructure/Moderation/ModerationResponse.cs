namespace DashiToon.Api.Infrastructure.Moderation;

public class ModerationResponse
{
    public List<ModerationResult> Results { get; init; } = [];
}

public class ModerationResult
{
    public bool Flagged { get; init; }
    public Dictionary<string, bool> Categories { get; init; } = [];
    public Dictionary<string, float> CategoryScores { get; init; } = [];
}
