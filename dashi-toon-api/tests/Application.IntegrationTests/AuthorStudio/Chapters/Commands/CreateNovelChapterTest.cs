using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.AuthorStudio.Chapters.Commands;

using static Testing;

[Collection("Serialize")]
public class CreateNovelChapterTest : BaseIntegrationTest
{
    public CreateNovelChapterTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task CreateNovelChapterShouldPersistToDatabase()
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

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "TestName", "TestIntroduction"));

        CreateNovelChapterCommand createChapterCommand = new(
            seriesId,
            volumeId,
            "TestTitle",
            "TestThumbnail",
            "<p>My life be like ooooh ahhhhhh</p>",
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
        chapter.GetCurrentVersion().Content.Should().Be("<p>My life be like ooooh ahhhhhh</p>");
        chapter.GetCurrentVersion().Note.Should().Be("TestNote");
        chapter.PublishedDate.Should().BeNull();
        chapter.Created.Should().BeWithin(TimeSpan.FromMilliseconds(100));
        chapter.CreatedBy.Should().Be(userId);
    }

    [Fact]
    public async Task CreateNovelChapterShouldRequireExistingSeries()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        CreateNovelChapterCommand createChapterCommand = new(
            1,
            1,
            "TestTitle",
            "TestThumbnail",
            "<p>My life be like ooooh ahhhhhh</p>",
            "TestNote"
        );

        // Act
        await FluentActions.Invoking(() => SendAsync(createChapterCommand)).Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateNovelChapterShouldRequireValidUser()
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

        await RunAsUserAsync("AnotherGuy", "Itsapassword1!", []);

        CreateNovelChapterCommand createChapterCommand = new(
            seriesId,
            1,
            "TestTitle",
            "TestThumbnail",
            "<p>My life be like ooooh ahhhhhh</p>",
            "TestNote"
        );

        // Act
        await FluentActions.Invoking(() => SendAsync(createChapterCommand)).Should()
            .ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task CreateNovelChapterShouldRequireSeriesOfTypeNovel()
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

        CreateNovelChapterCommand createChapterCommand = new(
            seriesId,
            1,
            "TestTitle",
            "TestThumbnail",
            "<p>My life be like ooooh ahhhhhh</p>",
            "TestNote"
        );

        // Act
        await FluentActions.Invoking(() => SendAsync(createChapterCommand)).Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateNovelChapterShouldRequireExistingVolume()
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

        CreateNovelChapterCommand createChapterCommand = new(
            seriesId,
            1,
            "TestTitle",
            "TestThumbnail",
            "<p>My life be like ooooh ahhhhhh</p>",
            "TestNote"
        );

        // Act
        await FluentActions.Invoking(() => SendAsync(createChapterCommand)).Should().ThrowAsync<NotFoundException>();
    }
}
