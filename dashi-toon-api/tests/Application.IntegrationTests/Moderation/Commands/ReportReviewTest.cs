using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Moderation.Commands.ReportReview;
using DashiToon.Api.Application.Moderation.Queries.GetReviewReports;
using DashiToon.Api.Application.Reviews.Commands.WriteSeriesReviews;
using DashiToon.Api.Application.Reviews.Models;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.Moderation.Commands;

using static Testing;

[Collection("Serialize")]
public class ReportReviewTest : BaseIntegrationTest
{
    public ReportReviewTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task WriteReviewShouldTriggerSystemReport()
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

        // Act
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

        // Assert
        await RunAsModeratorAsync();
        PaginatedList<ReviewReportVm>? reports = await SendAsync(new GetReviewReportsQuery());

        reports.Items.First().ReviewId.Should().Be(review.Id);
        reports.Items.First().ReviewContent.Should().Be(
            """
            Nice Content Nice Content Nice Content Nice Content  
            Nice Content Nice Content Nice Content Nice Content  
            Nice Content Nice Content Nice Content Nice Content  
            Nice Content Nice Content Nice Content Nice Content  
            Nice Content Nice Content Nice Content Nice Content  
            """);
        reports.Items.First().ReviewUser.Should().Be("test@local");
        reports.Items.First().SeriesId.Should().Be(seriesId);
        reports.Items.First().SeriesTitle.Should().Be("TestTitle");
        reports.Items.First().Reports[0].ReportedByUsername.Should().Be("Hệ thống");
        reports.Items.First().Reports[0].Reason.Should().Be("Hệ thống kiểm duyệt tự động");
        reports.Items.First().Reports[0].ReportedAt.Should().NotBeNull();
        reports.Items.First().Reports[0].Flagged.Should().BeTrue();
        reports.Items.First().Reports[0].FlaggedCategories.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ReportSeriesShouldCreateUserReportReview()
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

        // Act
        await SendAsync(new ReportReviewCommand(review.Id, "Test Reason"));

        // Assert
        await RunAsModeratorAsync();
        PaginatedList<ReviewReportVm>? reports = await SendAsync(new GetReviewReportsQuery());

        reports.Items.First().ReviewId.Should().Be(review.Id);
        reports.Items.First().ReviewContent.Should().Be(
            """
            Nice Content Nice Content Nice Content Nice Content  
            Nice Content Nice Content Nice Content Nice Content  
            Nice Content Nice Content Nice Content Nice Content  
            Nice Content Nice Content Nice Content Nice Content  
            Nice Content Nice Content Nice Content Nice Content  
            """);
        reports.Items.First().ReviewUser.Should().Be("test@local");
        reports.Items.First().SeriesId.Should().Be(seriesId);
        reports.Items.First().SeriesTitle.Should().Be("TestTitle");
        reports.Items.First().Reports[1].ReportedByUsername.Should().Be("test@local");
        reports.Items.First().Reports[1].Reason.Should().Be("Test Reason");
        reports.Items.First().Reports[1].ReportedAt.Should().NotBeNull();
        reports.Items.First().Reports[1].Flagged.Should().BeNull();
        reports.Items.First().Reports[1].FlaggedCategories.Should().BeEmpty();
    }
}
