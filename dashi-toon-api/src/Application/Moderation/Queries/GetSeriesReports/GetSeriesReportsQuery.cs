using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Mappings;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Moderation.Model;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Moderation.Queries.GetSeriesReports;

[Authorize(Roles = Roles.Moderator)]
public sealed record GetSeriesReportsQuery(int PageNumber = 1, int PageSize = 10)
    : IRequest<PaginatedList<SeriesReportVm>>;

public sealed class GetSeriesReportsQueryHandler
    : IRequestHandler<GetSeriesReportsQuery, PaginatedList<SeriesReportVm>>
{
    private readonly IReportRepository _reportRepository;
    private readonly IImageStore _imageStore;

    public GetSeriesReportsQueryHandler(IReportRepository reportRepository, IImageStore imageStore)
    {
        _reportRepository = reportRepository;
        _imageStore = imageStore;
    }

    public async Task<PaginatedList<SeriesReportVm>> Handle(
        GetSeriesReportsQuery request,
        CancellationToken cancellationToken)
    {
        (int count, IEnumerable<SeriesReportDto> reports) =
            await _reportRepository.FindSeriesReports(request.PageNumber, request.PageSize);

        List<SeriesReportDto>? seriesReports = reports.ToList();

        foreach (SeriesReportDto? report in seriesReports)
        {
            report.SeriesThumbnail = string.IsNullOrEmpty(report.SeriesThumbnail)
                ? await _imageStore.GetUrl("thumbnails/default.png", DateTime.UtcNow.AddMinutes(2))
                : await _imageStore.GetUrl($"thumbnails/{report.SeriesThumbnail}", DateTime.UtcNow.AddMinutes(2));
        }

        return new PaginatedList<SeriesReportVm>(
            seriesReports.Select(sr => new SeriesReportVm(
                sr.SeriesId,
                sr.SeriesTitle,
                sr.SeriesThumbnail,
                sr.SeriesSynopsis,
                sr.SeriesAuthor,
                sr.Reports.Select(r => new ReportVm(
                    r.ReportedUser,
                    r.Reason,
                    r.ReportedAt,
                    r.AnalysisFlagged,
                    r.AnalysisFlaggedCategories.ToDictionary(
                        c => c.Category,
                        c => c.Score
                    )
                )).ToList()
            )).ToList(),
            count,
            request.PageNumber,
            request.PageSize
        );
    }
}
