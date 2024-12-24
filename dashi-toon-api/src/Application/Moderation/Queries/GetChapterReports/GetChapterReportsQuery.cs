using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Mappings;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Moderation.Model;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Moderation.Queries.GetChapterReports;

[Authorize(Roles = Roles.Moderator)]
public sealed record GetChapterReportsQuery(int PageNumber = 1, int PageSize = 10)
    : IRequest<PaginatedList<ChapterReportVm>>;

public sealed class GetChapterReportsQueryHandler
    : IRequestHandler<GetChapterReportsQuery, PaginatedList<ChapterReportVm>>
{
    private readonly IReportRepository _reportRepository;

    public GetChapterReportsQueryHandler(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<PaginatedList<ChapterReportVm>> Handle(
        GetChapterReportsQuery request,
        CancellationToken cancellationToken)
    {
        (int count, IEnumerable<ChapterReportDto> reports) = await _reportRepository.FindChapterReports(
            request.PageNumber,
            request.PageSize);

        return new PaginatedList<ChapterReportVm>(
            reports.Select(cr => new ChapterReportVm(
                cr.ChapterId,
                cr.ChapterNumber,
                cr.VolumeId,
                cr.VolumeNumber,
                cr.SeriesId,
                cr.SeriesTitle,
                cr.SeriesType,
                cr.SeriesAuthor,
                cr.Reports.Select(r => new ReportVm(
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
