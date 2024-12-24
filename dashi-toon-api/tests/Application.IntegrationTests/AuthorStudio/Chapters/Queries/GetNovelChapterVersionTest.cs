using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Queries.GetNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Queries.GetNovelChapterVersions;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.AuthorStudio.Chapters.Queries;

using static Testing;

[Collection("Serialize")]
public class GetNovelChapterVersionTest : BaseIntegrationTest
{
    public GetNovelChapterVersionTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetNovelChapterVersionShouldReturnVersions()
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

        int chapterId = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            """<img src="" data-img-name="bruh.png"/>""",
            "TestNote1"
        ));

        GetNovelChapterVersionsQuery getComicChapterVersionQuery = new(
            seriesId,
            volumeId,
            chapterId,
            string.Empty,
            false,
            null,
            null);

        // Act
        PaginatedList<NovelChapterVersionVm> chapterVersions = await SendAsync(getComicChapterVersionQuery);

        // Assert
        chapterVersions.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetNovelChapterVersionShouldRequireExistingSeries()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new GetNovelChapterVersionsQuery(
                1,
                1,
                1,
                null,
                true,
                null,
                null)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetNovelChapterVersionShouldOnlyAllowAuthor()
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

        await RunAsUserAsync("AnotherGuy", "Itsapassword1!", []);

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new GetNovelChapterVersionsQuery(
                seriesId,
                1,
                1,
                null,
                true,
                null,
                null)))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task GetNovelChapterVersionShouldRequireSeriesOfTypeNovel()
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
        await FluentActions.Invoking(() => SendAsync(new GetNovelChapterVersionsQuery(
                seriesId,
                1,
                1,
                null,
                true,
                null,
                null)))
            .Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task GetNovelChapterVersionShouldRequireExistingVolume()
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

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new GetNovelChapterVersionsQuery(
                seriesId,
                1,
                1,
                null,
                true,
                null,
                null)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetNovelChapterVersionShouldRequireExistingChapter()
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

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new GetNovelChapterVersionsQuery(
                seriesId,
                volumeId,
                1,
                null,
                true,
                null,
                null)))
            .Should().ThrowAsync<NotFoundException>();
    }
}
