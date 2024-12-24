using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Moderation.Commands.ReportSeries;
using DashiToon.Api.Application.Moderation.Queries.GetSeriesReports;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.Moderation.Commands;

using static Testing;

[Collection("Serialize")]
public class ReportSeriesTest : BaseIntegrationTest
{
    public ReportSeriesTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task PublishSeriesShouldTriggerSystemReport()
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

        // Assert
        await RunAsModeratorAsync();
        PaginatedList<SeriesReportVm>? reports = await SendAsync(new GetSeriesReportsQuery());

        reports.Items.First().SeriesId.Should().Be(seriesId);
        reports.Items.First().SeriesTitle.Should().Be("TestTitle");
        reports.Items.First().SeriesThumbnail.Should().Be("thumbnails/TestThumbnail");
        reports.Items.First().SeriesSynopsis.Should().Be("TestSynopsis");
        reports.Items.First().SeriesAuthor.Should().Be("test@local");
        reports.Items.First().Reports[0].ReportedByUsername.Should().Be("Hệ thống");
        reports.Items.First().Reports[0].Reason.Should().Be("Hệ thống kiểm duyệt tự động");
        reports.Items.First().Reports[0].ReportedAt.Should().NotBeNull();
        reports.Items.First().Reports[0].Flagged.Should().BeTrue();
        reports.Items.First().Reports[0].FlaggedCategories.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ReportSeriesShouldCreateUserReportSeries()
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
        await SendAsync(new ReportSeriesCommand(seriesId, "Test Reason"));

        // Assert
        await RunAsModeratorAsync();
        PaginatedList<SeriesReportVm>? reports = await SendAsync(new GetSeriesReportsQuery());

        reports.Items.First().SeriesId.Should().Be(seriesId);
        reports.Items.First().SeriesTitle.Should().Be("TestTitle");
        reports.Items.First().SeriesThumbnail.Should().Be("thumbnails/TestThumbnail");
        reports.Items.First().SeriesSynopsis.Should().Be("TestSynopsis");
        reports.Items.First().SeriesAuthor.Should().Be("test@local");
        reports.Items.First().Reports[0].ReportedByUsername.Should().Be("test@local");
        reports.Items.First().Reports[0].Reason.Should().Be("Test Reason");
        reports.Items.First().Reports[0].ReportedAt.Should().NotBeNull();
        reports.Items.First().Reports[0].Flagged.Should().BeNull();
        reports.Items.First().Reports[0].FlaggedCategories.Should().BeEmpty();
    }
}
