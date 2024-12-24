using Bogus;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.ReadContent.Commands.IncrementViewCount;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.ReadContent.Commands;

using static Testing;

[Collection("Serializing")]
public class IncrementViewCountTest : BaseIntegrationTest
{
    public IncrementViewCountTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task IncrementViewCountShouldIncrementViewCountFromUserOfAChapterOnceEvery30Seconds()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        int seriesId = await SendAsync(new CreateSeriesCommand(
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesType.Novel,
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

        int chapterId = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            """<img src="" data-img-name="bruh.png"/>""",
            "TestNote1"
        ));

        LogOut();

        Faker? faker = new();

        string? ipAddress1 = faker.Internet.Ip();
        string? ipAddress2 = faker.Internet.Ip();

        // Act
        await SendAsync(new IncrementViewCountCommand(ipAddress1, chapterId));
        await SendAsync(new IncrementViewCountCommand(ipAddress1, chapterId));

        await SendAsync(new IncrementViewCountCommand(ipAddress2, chapterId));

        await RunAsUserAsync("User1", "Bruh@1311", []);
        await SendAsync(new IncrementViewCountCommand(null, chapterId));

        await Task.Delay(TimeSpan.FromSeconds(31));

        // Assert
        Chapter? chapter = await FindAsync<Chapter>(chapterId);

        chapter.Should().NotBeNull();
        chapter!.ViewCount.Should().Be(3);
        chapter.Analytics[0].ViewCount.Should().Be(3);
    }
}
