using DashiToon.Api.Application.Moderation.Model;

namespace DashiToon.Api.Application.Moderation.Queries.GetCommentReports;

public sealed record CommentReportVm(
    Guid CommentId,
    string CommentContent,
    string CommentUser,
    string ChapterNumber,
    string VolumeNumber,
    string SeriesTitle,
    List<ReportVm> Reports
);
