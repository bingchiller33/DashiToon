using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.PublishChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Comments.Commands.WriteComment;
using DashiToon.Api.Application.Comments.Models;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Moderation.Commands.DimissReports;
using DashiToon.Api.Application.Moderation.Commands.ReportChapter;
using DashiToon.Api.Application.Moderation.Commands.ReportComment;
using DashiToon.Api.Application.Moderation.Commands.ReportReview;
using DashiToon.Api.Application.Moderation.Commands.ReportSeries;
using DashiToon.Api.Application.Moderation.Queries.GetChapterReports;
using DashiToon.Api.Application.Moderation.Queries.GetCommentReports;
using DashiToon.Api.Application.Moderation.Queries.GetReviewReports;
using DashiToon.Api.Application.Reviews.Commands.WriteSeriesReviews;
using DashiToon.Api.Application.Reviews.Models;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.Moderation.Commands;

using static Testing;

[Collection("Serialize")]
public class DismissReportTest : BaseIntegrationTest
{
    public DismissReportTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task DismissSeriesReportShouldUpdateAllReportsStatus()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

        int seriesId = await SendAsync(new CreateSeriesCommand(
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesType.Novel,
            [1, 2, 3],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 1),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 1),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 1)
            ],
            [],
            [],
            null));

        await SendAsync(new UpdateSeriesCommand(
            seriesId,
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesStatus.Ongoing,
            [1, 2, 3],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 1),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 1),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 1)
            ],
            [],
            [],
            null));

        await SendAsync(new ReportSeriesCommand(seriesId, "Test Reason"));

        await RunAsModeratorAsync();
        // Act
        await SendAsync(new DismissReportsCommand(seriesId.ToString(), ReportType.Series));

        // Assert
        PaginatedList<CommentReportVm>? reports = await SendAsync(new GetCommentReportsQuery());
        reports.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task DismissCommentReportShouldUpdateAllReportsStatus()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

        int seriesId = await SendAsync(new CreateSeriesCommand(
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesType.Novel,
            [1, 2, 3],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 1),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 1),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 1)
            ],
            [],
            [],
            null));

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "TestName", "TestIntroduction"));

        int chapterId = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId,
            "TestTitle",
            "TestThumbnail",
            "<p>My life be like ooooh ahhhhhh</p>",
            "TestNote"
        ));

        CommentVm? comment = await SendAsync(new WriteCommentCommand(
            chapterId,
            "Test Content"));

        await SendAsync(new ReportCommentCommand(comment.Id, "Test Reason"));

        await RunAsModeratorAsync();
        // Act
        await SendAsync(new DismissReportsCommand(comment.Id.ToString(), ReportType.Comment));

        // Assert
        PaginatedList<CommentReportVm>? reports = await SendAsync(new GetCommentReportsQuery());
        reports.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task DismissReviewReportShouldUpdateAllReportsStatus()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

        int seriesId = await SendAsync(new CreateSeriesCommand(
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesType.Novel,
            [1, 2, 3],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 1),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 1),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 1)
            ],
            [],
            [],
            null));

        ReviewVm? review = await SendAsync(new WriteSeriesReviewCommand(
            seriesId,
            """
            Nice Content Nice Content Nice Content Nice Content  
            Nice Content Nice Content Nice Content Nice Content  
            Nice Content Nice Content Nice Content Nice Content  
            Nice Content Nice Content Nice Content Nice Content  
            Nice Content Nice Content Nice Content Nice Content  
            """,
            true));

        await SendAsync(new ReportReviewCommand(review.Id, "Test Reason"));

        await RunAsModeratorAsync();
        // Act
        await SendAsync(new DismissReportsCommand(review.Id.ToString(), ReportType.Review));

        // Assert
        PaginatedList<ReviewReportVm>? reports = await SendAsync(new GetReviewReportsQuery());
        reports.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task DismissChapterReportShouldUpdateAllReportsStatus()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

        int seriesId = await SendAsync(new CreateSeriesCommand(
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesType.Novel,
            [1, 2, 3],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 1),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 1),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 1)
            ],
            [],
            ["Auth 1"],
            null));

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "TestName", "TestIntroduction"));

        int chapterId = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId,
            "TestTitle",
            "TestThumbnail",
            "<p>My life be like ooooh ahhhhhh</p>",
            "TestNote"
        ));

        await SendAsync(new PublishChapterCommand(
            seriesId,
            volumeId,
            chapterId));

        await SendAsync(new ReportChapterCommand(chapterId, "Test Reason"));

        await RunAsModeratorAsync();
        // Act
        await SendAsync(new DismissReportsCommand(chapterId.ToString(), ReportType.Content));

        // Assert
        PaginatedList<ChapterReportVm>? reports = await SendAsync(new GetChapterReportsQuery());
        reports.TotalCount.Should().Be(0);
    }
}
