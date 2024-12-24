using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateComicChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.UpdateChapterVersion;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.AuthorStudio.Chapters.Commands;

using static Testing;

[Collection("Serialize")]
public class UpdateChapterVersionTest : BaseIntegrationTest
{
    public UpdateChapterVersionTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task UpdateChapterVersionShouldChangeVersionName()
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

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "TestName", "TestIntroduction"));

        int chapterId = await SendAsync(new CreateComicChapterCommand(
            seriesId,
            volumeId,
            "TestTitle",
            "TestThumbnail",
            ["image1.png", "image2.png"],
            "TestNote"
        ));

        Guid version = (await FindAsync<Chapter>(chapterId))!.CurrentVersionId;

        // Act
        await SendAsync(new UpdateChapterVersionCommand(seriesId, volumeId, chapterId, version, "New Name"));

        // Assert
        Chapter? chapter = await FindAsync<Chapter>(chapterId);

        chapter.Should().NotBeNull();
        chapter!.GetCurrentVersion().VersionName.Should().Be("New Name");
    }

    [Fact]
    public async Task UpdateChapterVersionShouldRequireExistingSeries()
    {
        // AAA
        await RunAsDefaultUserAsync();

        await FluentActions
            .Invoking(() => SendAsync(new UpdateChapterVersionCommand(1, 1, 1, Guid.NewGuid(), "New Name")))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateChapterVersionShouldRequireExistingVolume()
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

        // AA
        await FluentActions
            .Invoking(() => SendAsync(new UpdateChapterVersionCommand(seriesId, 1, 1, Guid.NewGuid(), "New Name")))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateChapterVersionShouldRequireExistingChapter()
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

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "TestName", "TestIntroduction"));

        // AA
        await FluentActions
            .Invoking(() =>
                SendAsync(new UpdateChapterVersionCommand(seriesId, volumeId, 1, Guid.NewGuid(), "New Name")))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateChapterVersionShouldAllowOnlyAuthorToModify()
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

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "TestName", "TestIntroduction"));

        await RunAsUserAsync("TestUser@test.com", "TestPass@123!", []);
        // AA

        await FluentActions
            .Invoking(() =>
                SendAsync(new UpdateChapterVersionCommand(seriesId, volumeId, 1, Guid.NewGuid(), "New Name")))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }
}
