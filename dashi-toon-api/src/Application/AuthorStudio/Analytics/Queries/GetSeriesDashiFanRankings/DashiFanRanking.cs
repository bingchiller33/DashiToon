namespace DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesDashiFanRankings;

public sealed record DashiFanRanking(
    Guid Id,
    string Name,
    int Data,
    int? Compare,
    int Rank
);

public sealed class DashiFanRankingDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public int Data { get; init; }
    public int? Compare { get; init; }
    public int Rank { get; init; }
}
