using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateComicChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Queries.GetChapters;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.AuthorStudio.Chapters.Queries;

using static Testing;

[Collection("Serialize")]
public class GetChaptersTest : BaseIntegrationTest
{
    public GetChaptersTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetChaptersShouldReturnAllChapters()
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

        await SendAsync(new CreateComicChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            ["image1.png", "image2.png"],
            "TestNote1"
        ));
        await SendAsync(new CreateComicChapterCommand(
            seriesId,
            volumeId,
            "TestTitle2",
            "TestThumbnail2",
            ["image1.png", "image2.png"],
            "TestNote2"
        ));

        // Act
        PaginatedList<ChapterVm> result = await SendAsync(new GetChaptersQuery(seriesId, volumeId, null, null));

        // Assert
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetChaptersShouldRequireExistingSeries()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new GetChaptersQuery(1, 1, null, null)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetChaptersShouldOnlyAllowedAuthor()
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

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new GetChaptersQuery(seriesId, 1, null, null)))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task GetChaptersShouldRequireExistingVolume()
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
        await FluentActions.Invoking(() => SendAsync(new GetChaptersQuery(seriesId, 1, null, null)))
            .Should().ThrowAsync<NotFoundException>();
    }
}
