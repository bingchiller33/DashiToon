namespace DashiToon.Api.Application.AuthorStudio.Analytics.Models;

public sealed record ChartData(
    string Time,
    int Current,
    int? Compare
);

public sealed class ChartDataDto
{
    public string Time { get; init; } = null!;
    public int Data { get; init; }
}
