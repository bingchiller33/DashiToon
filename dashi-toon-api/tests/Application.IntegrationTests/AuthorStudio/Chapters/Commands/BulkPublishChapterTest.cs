using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.BulkPublishChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.AuthorStudio.Chapters.Commands;

using static Testing;

[Collection("Serialize")]
public class BulkPublishChapterTest : BaseIntegrationTest
{
    public BulkPublishChapterTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task BulkPublishChapterShouldSuccess()
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

        BulkPublishChapterCommand bulkPublishChapterCommand = new(seriesId, volumeId, [chapterId1, chapterId2]);

        // Act
        await SendAsync(bulkPublishChapterCommand);

        // Assert
        Chapter? chapter1 = await FindAsync<Chapter>(chapterId1);
        Chapter? chapter2 = await FindAsync<Chapter>(chapterId2);

        chapter1.Should().NotBeNull();
        chapter1!.PublishedDate.Should().BeWithin(TimeSpan.FromMilliseconds(100));
        chapter1.GetCurrentVersion().Status.Should().Be(ChapterStatus.Published);

        chapter2.Should().NotBeNull();
        chapter2!.PublishedDate.Should().BeWithin(TimeSpan.FromMilliseconds(100));
        chapter2.GetCurrentVersion().Status.Should().Be(ChapterStatus.Published);
    }

    [Fact]
    public async Task BulkPublishChapterShouldRequireExistingSeries()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        // Act
        await FluentActions.Invoking(() => SendAsync(new BulkPublishChapterCommand(1, 1, [1])))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task BulkPublishChapterShouldRequireValidUser()
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
        await FluentActions.Invoking(() => SendAsync(new BulkPublishChapterCommand(seriesId, 1, [1])))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task BulkPublishChapterShouldRequireExistingVolume()
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
        await FluentActions.Invoking(() => SendAsync(new BulkPublishChapterCommand(seriesId, 1, [1])))
            .Should().ThrowAsync<NotFoundException>();
    }
}
