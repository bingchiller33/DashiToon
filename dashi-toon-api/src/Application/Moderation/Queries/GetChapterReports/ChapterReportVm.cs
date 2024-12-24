using DashiToon.Api.Application.Moderation.Model;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.Moderation.Queries.GetChapterReports;

public sealed record ChapterReportVm(
    int ChapterId,
    int ChapterNumber,
    int VolumeId,
    int VolumeNumber,
    int SeriesId,
    string SeriesTitle,
    SeriesType SeriesType,
    string SeriesAuthor,
    List<ReportVm> Reports);
