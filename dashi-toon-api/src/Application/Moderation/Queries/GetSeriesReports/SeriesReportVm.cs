using DashiToon.Api.Application.Moderation.Model;

namespace DashiToon.Api.Application.Moderation.Queries.GetSeriesReports;

public sealed record SeriesReportVm(
    int SeriesId,
    string SeriesTitle,
    string SeriesThumbnail,
    string SeriesSynopsis,
    string SeriesAuthor,
    List<ReportVm> Reports);
