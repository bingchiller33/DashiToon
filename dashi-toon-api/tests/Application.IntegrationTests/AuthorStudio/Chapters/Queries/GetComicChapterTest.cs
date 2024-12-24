using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateComicChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Queries.GetComicChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.AuthorStudio.Chapters.Queries;

using static Testing;

[Collection("Serialize")]
public class GetComicChapterTest : BaseIntegrationTest
{
    public GetComicChapterTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetComicChapterShouldReturnChapterDetail()
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
            "TestTitle1",
            "TestThumbnail1",
            ["image1.png", "image2.png"],
            "TestNote1"
        ));

        GetComicChapterQuery getComicChapterQuery = new(seriesId, volumeId, chapterId);
        // Act
        ComicChapterDetailVm chapterDetail = await SendAsync(getComicChapterQuery);

        // Assert
        chapterDetail.Id.Should().Be(chapterId);
        chapterDetail.Title.Should().Be("TestTitle1");
        chapterDetail.Thumbnail.Should().Be("TestThumbnail1");
        chapterDetail.Note.Should().Be("TestNote1");
        chapterDetail.Content.Should().HaveCount(2);
        chapterDetail.Status.Should().Be(ChapterStatus.Draft);
        chapterDetail.PublishedDate.Should().BeNull();
    }

    [Fact]
    public async Task GetComicChapterShouldRequireSeriesOfTypeComic()
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
            "<p>No shot</p>",
            "TestNote1"
        ));

        // Act
        await FluentActions.Invoking(() => SendAsync(new GetComicChapterQuery(seriesId, volumeId, chapterId)))
            .Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task GetComicChapterShouldRequireExistingSeries()
    {
        // Act
        await RunAsDefaultUserAsync();

        await FluentActions.Invoking(() => SendAsync(new GetComicChapterQuery(1, 1, 1)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetComicChapterShouldOnlyAllowAuthor()
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
        await FluentActions.Invoking(() => SendAsync(new GetComicChapterQuery(seriesId, 1, 1)))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task GetComicChapterShouldRequireExistingVolume()
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
        await FluentActions.Invoking(() => SendAsync(new GetComicChapterQuery(seriesId, 1, 1)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetComicChapterShouldRequireExistingChapter()
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

        // Act
        await FluentActions.Invoking(() => SendAsync(new GetComicChapterQuery(seriesId, volumeId, 1)))
            .Should().ThrowAsync<NotFoundException>();
    }
}
