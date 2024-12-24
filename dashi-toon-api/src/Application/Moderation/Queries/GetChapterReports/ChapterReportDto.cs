using DashiToon.Api.Application.Moderation.Model;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.Moderation.Queries.GetChapterReports;

public class ChapterReportDto
{
    public int ChapterId { get; init; }
    public int ChapterNumber { get; init; }
    public int VolumeId { get; init; }
    public int VolumeNumber { get; init; }
    public int SeriesId { get; init; }
    public string SeriesTitle { get; init; } = null!;
    public SeriesType SeriesType { get; init; }
    public string SeriesAuthor { get; init; } = null!;
    public List<ReportDto> Reports { get; init; } = [];
}
