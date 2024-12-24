using DashiToon.Api.Application.Moderation.Model;

namespace DashiToon.Api.Application.Moderation.Queries.GetReviewReports;

public sealed record ReviewReportVm(
    Guid ReviewId,
    string ReviewContent,
    string ReviewUser,
    int SeriesId,
    string SeriesTitle,
    List<ReportVm> Reports);
