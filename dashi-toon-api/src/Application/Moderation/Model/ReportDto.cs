using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Moderation.Model;

public class ReportDto
{
    public string ReportedUser { get; init; } = null!;
    public string Reason { get; init; } = null!;
    public string ReportedAt { get; init; } = null!;
    public bool? AnalysisFlagged { get; init; }
    public string? AnalysisFlaggedCategoriesString { get; init; }
    public List<CategoryScore> AnalysisFlaggedCategories { get; init; } = [];
}
