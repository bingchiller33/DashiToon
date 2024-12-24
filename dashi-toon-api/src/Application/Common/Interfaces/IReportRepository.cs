using DashiToon.Api.Application.Moderation.Queries.GetChapterReports;
using DashiToon.Api.Application.Moderation.Queries.GetCommentReports;
using DashiToon.Api.Application.Moderation.Queries.GetReviewReports;
using DashiToon.Api.Application.Moderation.Queries.GetSeriesReports;

namespace DashiToon.Api.Application.Common.Interfaces;

public interface IReportRepository
{
    public Task<(int Count, IEnumerable<CommentReportDto> Result)> FindCommentReports(int pageNumber, int pageSize);
    public Task<(int Count, IEnumerable<ReviewReportDto> Result)> FindReviewReports(int pageNumber, int pageSize);
    public Task<(int Count, IEnumerable<ChapterReportDto> Result)> FindChapterReports(int pageNumber, int pageSize);
    public Task<(int Count, IEnumerable<SeriesReportDto> Result)> FindSeriesReports(int pageNumber, int pageSize);
}
