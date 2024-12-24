using DashiToon.Api.Application.Moderation.Model;

namespace DashiToon.Api.Application.Moderation.Queries.GetCommentReports;

public class CommentReportDto
{
    public Guid CommentId { get; init; }
    public string CommentContent { get; init; } = null!;
    public string CommentUser { get; init; } = null!;
    public string ChapterNumber { get; init; } = null!;
    public string VolumeNumber { get; init; } = null!;
    public string SeriesTitle { get; init; } = null!;
    public List<ReportDto> Reports { get; set; } = [];
}
