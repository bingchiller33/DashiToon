using DashiToon.Api.Application.Moderation.Model;

namespace DashiToon.Api.Application.Moderation.Queries.GetSeriesReports;

public class SeriesReportDto
{
    public int SeriesId { get; init; }
    public string SeriesTitle { get; init; } = null!;
    public string SeriesThumbnail { get; set; } = null!;
    public string SeriesSynopsis { get; init; } = null!;
    public string SeriesAuthor { get; init; } = null!;
    public List<ReportDto> Reports { get; init; } = [];
}
