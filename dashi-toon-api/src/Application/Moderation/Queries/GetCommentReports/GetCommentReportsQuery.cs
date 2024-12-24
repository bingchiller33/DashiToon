using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Mappings;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Moderation.Model;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Moderation.Queries.GetCommentReports;

[Authorize(Roles = Roles.Moderator)]
public sealed record GetCommentReportsQuery(int PageNumber = 1, int PageSize = 10)
    : IRequest<PaginatedList<CommentReportVm>>;

public sealed class GetCommentReportsQueryHandler
    : IRequestHandler<GetCommentReportsQuery, PaginatedList<CommentReportVm>>
{
    private readonly IReportRepository _reportRepository;

    public GetCommentReportsQueryHandler(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<PaginatedList<CommentReportVm>> Handle(
        GetCommentReportsQuery request,
        CancellationToken cancellationToken)
    {
        (int count, IEnumerable<CommentReportDto> reports) = await _reportRepository.FindCommentReports(
            request.PageNumber,
            request.PageSize);

        return new PaginatedList<CommentReportVm>(
            reports.Select(cr => new CommentReportVm(
                cr.CommentId,
                cr.CommentContent,
                cr.CommentUser,
                cr.ChapterNumber,
                cr.VolumeNumber,
                cr.SeriesTitle,
                cr.Reports.Select(
                    r => new ReportVm(
                        r.ReportedUser,
                        r.Reason,
                        r.ReportedAt,
                        r.AnalysisFlagged,
                        r.AnalysisFlaggedCategories.ToDictionary(
                            a => a.Category,
                            a => a.Score
                        ))
                ).ToList()
            )).ToList(),
            count,
            request.PageNumber,
            request.PageSize
        );
    }
}
