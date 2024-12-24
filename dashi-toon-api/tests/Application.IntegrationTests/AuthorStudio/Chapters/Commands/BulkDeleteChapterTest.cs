using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.BulkDeleteChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using DashiToon.Api.Domain.Exceptions;

namespace Application.IntegrationTests.AuthorStudio.Chapters.Commands;

using static Testing;

[Collection("Serialize")]
public class BulkDeleteChapterTest : BaseIntegrationTest
{
    public BulkDeleteChapterTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task BulkDeleteChapterShouldRemoveFromDatabase()
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

        int chapterId1 = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            "<p>My life be like ooooh ahhhhhh</p>",
            "TestNote1"));

        int chapterId2 = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId,
            "TestTitle2",
            "TestThumbnail2",
            "<p>My life be like ooooh ahhhhhh</p>",
            "TestNote2"));

        BulkDeleteChapterCommand bulkDeleteChapterCommand = new(seriesId, volumeId, [chapterId1, chapterId2]);
        // Act
        await SendAsync(bulkDeleteChapterCommand);

        // Assert
        Chapter? chapter1 = await FindAsync<Chapter>(chapterId1);
        Chapter? chapter2 = await FindAsync<Chapter>(chapterId2);

        Volume? volume = await FindAsync<Volume>(volumeId);

        volume.Should().NotBeNull();
        volume!.ChapterCount.Should().Be(0);

        chapter1.Should().BeNull();
        chapter2.Should().BeNull();
    }

    [Fact]
    public async Task BulkDeleteChapterShouldRequireExistingSeries()
    {
        await RunAsDefaultUserAsync();

        // Act
        await FluentActions.Invoking(() => SendAsync(new BulkDeleteChapterCommand(1, 1, [1])))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task BulkDeleteChapterShouldRequireExistingVolume()
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
        await FluentActions.Invoking(() => SendAsync(new BulkDeleteChapterCommand(seriesId, 1, [1])))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task BulkDeleteChapterShouldRequireValidUser()
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
                new ContentQuestionnaire(ContentCategory.Nudity, 1),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 1),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 1)
            ],
            [],
            [],
            null));

        await RunAsUserAsync("AnotherGuy", "Itsapassword1!", []);

        // Act
        await FluentActions.Invoking(() => SendAsync(new BulkDeleteChapterCommand(seriesId, 1, [1])))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task BulkDeleteChapterShouldRequireExistingChapter()
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

        int chapterId1 = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            "<p>My life be like ooooh ahhhhhh</p>",
            "TestNote1"));

        // Act
        await FluentActions
            .Invoking(() => SendAsync(new BulkDeleteChapterCommand(seriesId, volumeId, [chapterId1, 10])))
            .Should().ThrowAsync<ChapterNotFoundException>();
    }
}
