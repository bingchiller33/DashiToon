using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Comments.Commands.WriteComment;
using DashiToon.Api.Application.Comments.Models;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Moderation.Commands.ReportComment;
using DashiToon.Api.Application.Moderation.Commands.ReportReview;
using DashiToon.Api.Application.Moderation.Queries.GetCommentReports;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.Moderation.Commands;

using static Testing;

[Collection("Serialize")]
public class ReportCommentTest : BaseIntegrationTest
{
    public ReportCommentTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task WriteCommentShouldTriggerSystemReport()
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

        // Act
        CommentVm? comment = await SendAsync(new WriteCommentCommand(
            chapterId,
            "Test Content"));

        // Assert
        await RunAsModeratorAsync();
        PaginatedList<CommentReportVm>? reports = await SendAsync(new GetCommentReportsQuery());

        reports.Items.First().CommentId.Should().Be(comment.Id);
        reports.Items.First().CommentContent.Should().Be("Test Content");
        reports.Items.First().CommentUser.Should().Be("test@local");
        reports.Items.First().ChapterNumber.Should().Be("1");
        reports.Items.First().VolumeNumber.Should().Be("1");
        reports.Items.First().SeriesTitle.Should().Be("TestTitle");
        reports.Items.First().Reports[0].ReportedByUsername.Should().Be("Hệ thống");
        reports.Items.First().Reports[0].Reason.Should().Be("Hệ thống kiểm duyệt tự động");
        reports.Items.First().Reports[0].ReportedAt.Should().NotBeNull();
        reports.Items.First().Reports[0].Flagged.Should().BeTrue();
        reports.Items.First().Reports[0].FlaggedCategories.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ReportSeriesShouldCreateUserReportComment()
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

        // Act
        await SendAsync(new ReportCommentCommand(comment.Id, "Test Reason"));

        // Assert
        await RunAsModeratorAsync();
        PaginatedList<CommentReportVm>? reports = await SendAsync(new GetCommentReportsQuery());

        reports.Items.First().CommentId.Should().Be(comment.Id);
        reports.Items.First().CommentContent.Should().Be("Test Content");
        reports.Items.First().CommentUser.Should().Be("test@local");
        reports.Items.First().ChapterNumber.Should().Be("1");
        reports.Items.First().VolumeNumber.Should().Be("1");
        reports.Items.First().SeriesTitle.Should().Be("TestTitle");
        reports.Items.First().Reports[1].ReportedByUsername.Should().Be("test@local");
        reports.Items.First().Reports[1].Reason.Should().Be("Test Reason");
        reports.Items.First().Reports[1].ReportedAt.Should().NotBeNull();
        reports.Items.First().Reports[1].Flagged.Should().BeNull();
        reports.Items.First().Reports[1].FlaggedCategories.Should().BeEmpty();
    }
}
