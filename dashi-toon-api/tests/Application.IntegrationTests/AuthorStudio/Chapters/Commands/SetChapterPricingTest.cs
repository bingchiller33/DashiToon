using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateComicChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.SetChapterPricing;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.AuthorStudio.Chapters.Commands;

using static Testing;

[Collection("Serialize")]
public class SetChapterPricingTest : BaseIntegrationTest
{
    public SetChapterPricingTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task SetChapterPricingShouldChangeChapterPrice()
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

        // Act
        await SendAsync(new SetChapterPricingCommand(seriesId, volumeId, chapterId, 30));

        // Assert
        Chapter? chapter = await FindAsync<Chapter>(chapterId);

        chapter.Should().NotBeNull();
        chapter!.KanaPrice.Should().Be(30);
    }

    [Fact]
    public async Task SetChapterPricingShouldRequireExistingSeries()
    {
        // Act & Assert
        await RunAsDefaultUserAsync();

        await FluentActions.Invoking(() => SendAsync(new SetChapterPricingCommand(1, 1, 1, 30)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task SetChapterPricingShouldOnlyAllowedAuthor()
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

        // Act & Assert
        await RunAsUserAsync("AnotherGuy", "Itsapassword1!", []);

        await FluentActions.Invoking(() => SendAsync(new SetChapterPricingCommand(seriesId, 1, 1, 30)))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task SetChapterPricingShouldRequireExistingVolume()
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

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new SetChapterPricingCommand(seriesId, 1, 1, 30)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task SetChapterPricingShouldRequireExistingChapter()
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

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new SetChapterPricingCommand(seriesId, volumeId, 1, 30)))
            .Should().ThrowAsync<NotFoundException>();
    }
}
