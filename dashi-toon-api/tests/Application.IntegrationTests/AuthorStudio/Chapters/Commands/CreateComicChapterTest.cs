using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateComicChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.AuthorStudio.Chapters.Commands;

using static Testing;

[Collection("Serialize")]
public class CreateComicChapterTest : BaseIntegrationTest
{
    public CreateComicChapterTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task CreateComicChapterShouldPersistToDatabase()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

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

        CreateComicChapterCommand createChapterCommand = new(
            seriesId,
            volumeId,
            "TestTitle",
            "TestThumbnail",
            ["image1.png", "image2.png"],
            "TestNote"
        );

        // Act
        int chapterId = await SendAsync(createChapterCommand);

        // Assert
        IApplicationDbContext context = GetContext();

        Chapter? chapter = context.Chapters.SingleOrDefault(c => c.Id == chapterId);

        chapter.Should().NotBeNull();
        chapter!.VolumeId.Should().Be(volumeId);
        chapter.Id.Should().Be(chapterId);
        chapter.ChapterNumber.Should().Be(1);
        chapter.GetCurrentVersion().Status.Should().Be(ChapterStatus.Draft);
        chapter.GetCurrentVersion().Title.Should().Be("TestTitle");
        chapter.GetCurrentVersion().Thumbnail.Should().Be("TestThumbnail");
        chapter.GetCurrentVersion().Content.Should().Be("""["image1.png","image2.png"]""");
        chapter.GetCurrentVersion().Note.Should().Be("TestNote");
        chapter.PublishedDate.Should().BeNull();
        chapter.Created.Should().BeWithin(TimeSpan.FromMilliseconds(100));
        chapter.CreatedBy.Should().Be(userId);
    }

    [Fact]
    public async Task CreateComicChapterShouldRequireExistingSeries()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        CreateComicChapterCommand createChapterCommand = new(
            1,
            1,
            "TestTitle",
            "TestThumbnail",
            ["image1.png", "image2.png"],
            "TestNote"
        );

        // Act
        await FluentActions.Invoking(() => SendAsync(createChapterCommand)).Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateComicChapterShouldRequireSeriesOfTypeComic()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

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

        CreateComicChapterCommand createChapterCommand = new(
            seriesId,
            1,
            "TestTitle",
            "TestThumbnail",
            ["image1.png", "image2.png"],
            "TestNote"
        );

        // Act
        await FluentActions.Invoking(() => SendAsync(createChapterCommand)).Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateComicChapterShouldRequireValidUser()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

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

        CreateComicChapterCommand createChapterCommand = new(
            seriesId,
            1,
            "TestTitle",
            "TestThumbnail",
            ["image1.png", "image2.png"],
            "TestNote"
        );

        // Act
        await FluentActions.Invoking(() => SendAsync(createChapterCommand)).Should()
            .ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task CreateComicChapterShouldRequireExistingVolume()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

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

        CreateComicChapterCommand createChapterCommand = new(
            seriesId,
            1,
            "TestTitle",
            "TestThumbnail",
            ["image1.png", "image2.png"],
            "TestNote"
        );

        // Act
        await FluentActions.Invoking(() => SendAsync(createChapterCommand)).Should().ThrowAsync<NotFoundException>();
    }
}
