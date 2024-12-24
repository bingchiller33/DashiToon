using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.PublishChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Moderation.Commands.ReportChapter;
using DashiToon.Api.Application.Moderation.Queries.GetChapterReports;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.Moderation.Commands;

using static Testing;

[Collection("Serialize")]
public class ReportChapterTest : BaseIntegrationTest
{
    public ReportChapterTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task PublishChapterShouldTriggerSystemReport()
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

        // Act
        await SendAsync(new PublishChapterCommand(
            seriesId,
            volumeId,
            chapterId));

        // Assert
        await RunAsModeratorAsync();
        PaginatedList<ChapterReportVm>? reports = await SendAsync(new GetChapterReportsQuery());

        reports.Items.First().ChapterId.Should().Be(chapterId);
        reports.Items.First().ChapterNumber.Should().Be(1);
        reports.Items.First().VolumeId.Should().Be(volumeId);
        reports.Items.First().VolumeNumber.Should().Be(1);
        reports.Items.First().SeriesId.Should().Be(seriesId);
        reports.Items.First().SeriesTitle.Should().Be("TestTitle");
        reports.Items.First().SeriesType.Should().Be(SeriesType.Novel);
        reports.Items.First().SeriesAuthor.Should().Be("test@local");
        reports.Items.First().Reports[0].ReportedByUsername.Should().Be("Hệ thống");
        reports.Items.First().Reports[0].Reason.Should().Be("Hệ thống kiểm duyệt tự động");
        reports.Items.First().Reports[0].ReportedAt.Should().NotBeNull();
        reports.Items.First().Reports[0].Flagged.Should().BeTrue();
        reports.Items.First().Reports[0].FlaggedCategories.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ReportChapterShouldCreateUserReportChapter()
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

        // Act
        await SendAsync(new ReportChapterCommand(chapterId, "Test Reason"));

        // Assert
        await RunAsModeratorAsync();
        PaginatedList<ChapterReportVm>? reports = await SendAsync(new GetChapterReportsQuery());

        reports.Items.First().ChapterId.Should().Be(chapterId);
        reports.Items.First().ChapterNumber.Should().Be(1);
        reports.Items.First().VolumeId.Should().Be(volumeId);
        reports.Items.First().VolumeNumber.Should().Be(1);
        reports.Items.First().SeriesId.Should().Be(seriesId);
        reports.Items.First().SeriesTitle.Should().Be("TestTitle");
        reports.Items.First().SeriesType.Should().Be(SeriesType.Novel);
        reports.Items.First().SeriesAuthor.Should().Be("test@local");
        reports.Items.First().Reports[0].ReportedByUsername.Should().Be("test@local");
        reports.Items.First().Reports[0].Reason.Should().Be("Test Reason");
        reports.Items.First().Reports[0].ReportedAt.Should().NotBeNull();
        reports.Items.First().Reports[0].Flagged.Should().BeNull();
        reports.Items.First().Reports[0].FlaggedCategories.Should().BeEmpty();
    }
}
