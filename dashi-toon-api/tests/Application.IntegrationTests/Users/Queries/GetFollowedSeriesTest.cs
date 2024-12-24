using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateComicChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.PublishChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.ReadContent.Queries.GetComicChapter;
using DashiToon.Api.Application.Users.Commands.FollowSeries;
using DashiToon.Api.Application.Users.Queries.GetFollowedSeries;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.Users.Queries;

using static Testing;

[Collection("Serialize")]
public class GetFollowedSeriesTest : BaseIntegrationTest
{
    public GetFollowedSeriesTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetFollowedSeriesShouldReturnFollowedSeries()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        int seriesId = await SendAsync(new CreateSeriesCommand(
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesType.Comic,
            [1, 2, 3],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 2),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 3),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
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
                new ContentQuestionnaire(ContentCategory.Nudity, 2),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 3),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
            ],
            [],
            [],
            null));

        await SendAsync(new FollowSeriesCommand(seriesId));

        // Act
        PaginatedList<FollowedSeriesVm>? series = await SendAsync(new GetFollowedSeriesQuery(null));

        // Assert
        series.TotalCount.Should().Be(1);
        series.Items.First().Title.Should().Be("TestTitle");
        series.Items.First().Type.Should().Be(SeriesType.Comic);
        series.Items.First().Status.Should().Be(SeriesStatus.Ongoing);
        series.Items.First().SeriesId.Should().Be(seriesId);
        series.Items.First().LatestVolumeReadId.Should().BeNull();
        series.Items.First().LatestChapterReadId.Should().BeNull();
        series.Items.First().Progress.Should().Be(0);
        series.Items.First().TotalChapters.Should().Be(0);
        series.Items.First().IsNotified.Should().BeTrue();
    }

    [Fact]
    public async Task GetFollowedSeriesShouldTrackReadingProgress()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        int seriesId = await SendAsync(new CreateSeriesCommand(
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesType.Comic,
            [1, 2, 3],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 2),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 3),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
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
                new ContentQuestionnaire(ContentCategory.Nudity, 2),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 3),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
            ],
            [],
            [],
            null));

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "Vol1", string.Empty));

        int chapterId = await SendAsync(new CreateComicChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            ["image1.png", "image2.png"],
            "TestNote1"
        ));

        await SendAsync(new PublishChapterCommand(seriesId, volumeId, chapterId));

        await SendAsync(new FollowSeriesCommand(seriesId));

        await SendAsync(new GetComicChapterQuery(seriesId, volumeId, chapterId));

        // Act
        PaginatedList<FollowedSeriesVm>? series = await SendAsync(new GetFollowedSeriesQuery(null));

        // Assert
        series.TotalCount.Should().Be(1);
        series.Items.First().Title.Should().Be("TestTitle");
        series.Items.First().Type.Should().Be(SeriesType.Comic);
        series.Items.First().Status.Should().Be(SeriesStatus.Ongoing);
        series.Items.First().SeriesId.Should().Be(seriesId);
        series.Items.First().LatestVolumeReadId.Should().Be(volumeId);
        series.Items.First().LatestChapterReadId.Should().Be(chapterId);
        series.Items.First().Progress.Should().Be(1);
        series.Items.First().TotalChapters.Should().Be(1);
        series.Items.First().IsNotified.Should().BeTrue();
    }
}
