using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateComicChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.PublishChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.ReadContent.Queries.GetComicChapter;
using DashiToon.Api.Application.Users.Commands.CheckinUser;
using DashiToon.Api.Application.Users.Commands.FollowSeries;
using DashiToon.Api.Application.Users.Queries.GetUserMetadatas;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.Users.Queries;

using static Testing;

[Collection("Serialize")]
public class GetUserMetadatasTest : BaseIntegrationTest
{
    public GetUserMetadatasTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetUserMetadataShouldReturnUserMetadata()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        // Act
        UserMetadata? metadata = await SendAsync(new GetUserMetadataQuery());

        // Assert
        metadata.IsCheckedIn.Should().BeFalse();
        metadata.CurrentDateChapterRead.Should().Be(0);
    }

    [Fact]
    public async Task CheckinShouldUpdateCheckInStatusInUserMetadata()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        await SendAsync(new CheckinUserCommand());

        // Act
        UserMetadata? metadata = await SendAsync(new GetUserMetadataQuery());

        // Assert
        metadata.IsCheckedIn.Should().BeTrue();
        metadata.CurrentDateChapterRead.Should().Be(0);
    }

    [Fact]
    public async Task ReadChapterShouldUpdateReadCountInUserMetadata()
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
        UserMetadata? metadata = await SendAsync(new GetUserMetadataQuery());

        // Assert
        metadata.IsCheckedIn.Should().BeFalse();
        metadata.CurrentDateChapterRead.Should().Be(1);
    }

    [Fact]
    public async Task ReadChapterShouldNotUpdateReadCountTwiceForTheSameChapterInUserMetadata()
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
        await SendAsync(new GetComicChapterQuery(seriesId, volumeId, chapterId));

        // Act
        UserMetadata? metadata = await SendAsync(new GetUserMetadataQuery());

        // Assert
        metadata.IsCheckedIn.Should().BeFalse();
        metadata.CurrentDateChapterRead.Should().Be(1);
    }
}
