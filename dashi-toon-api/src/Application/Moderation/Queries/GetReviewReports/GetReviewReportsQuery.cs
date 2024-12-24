using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Mappings;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Moderation.Model;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Moderation.Queries.GetReviewReports;

[Authorize(Roles = Roles.Moderator)]
public sealed record GetReviewReportsQuery(int PageNumber = 1, int PageSize = 10)
    : IRequest<PaginatedList<ReviewReportVm>>;

public sealed class GetReviewReportsQueryHandler
    : IRequestHandler<GetReviewReportsQuery, PaginatedList<ReviewReportVm>>
{
    private readonly IReportRepository _reportRepository;

    public GetReviewReportsQueryHandler(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<PaginatedList<ReviewReportVm>> Handle(
        GetReviewReportsQuery request,
        CancellationToken cancellationToken)
    {
        (int count, IEnumerable<ReviewReportDto> reports) = await _reportRepository
            .FindReviewReports(request.PageNumber, request.PageSize);

        return new PaginatedList<ReviewReportVm>(
            reports.Select(rr => new ReviewReportVm(
                rr.ReviewId,
                rr.ReviewContent,
                rr.ReviewUser,
                rr.SeriesId,
                rr.SeriesTitle,
                rr.Reports.Select(r => new ReportVm(
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
