using DashiToon.Api.Application.Moderation.Model;

namespace DashiToon.Api.Application.Moderation.Queries.GetReviewReports;

public class ReviewReportDto
{
    public Guid ReviewId { get; init; }
    public string ReviewContent { get; init; } = null!;
    public string ReviewUser { get; init; } = null!;
    public int SeriesId { get; init; }
    public string SeriesTitle { get; init; } = null!;
    public List<ReportDto> Reports { get; init; } = [];
}
