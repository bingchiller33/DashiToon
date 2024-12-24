using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateComicChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.UpdateComicChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.AuthorStudio.Chapters.Commands;

using static Testing;

[Collection("Serialize")]
public class UpdateComicChapterTest : BaseIntegrationTest
{
    public UpdateComicChapterTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task UpdateComicChapterShouldPersistToDatabase()
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

        int chapterId = await SendAsync(new CreateComicChapterCommand(
            seriesId,
            volumeId,
            "TestTitle",
            "TestThumbnail",
            ["img1.png", "img2.png"],
            "TestNote"
        ));

        UpdateComicChapterCommand updateChapterCommand = new(
            seriesId,
            volumeId,
            chapterId,
            "UpdatedTitle",
            "UpdatedThumbnail",
            ["img1.png", "img2.png", "img3.png"],
            "UpdatedNote");

        // Act
        await SendAsync(updateChapterCommand);

        // Assert
        IApplicationDbContext context = GetContext();

        Chapter? chapter = context.Chapters.SingleOrDefault(c => c.Id == chapterId);

        chapter!.ChapterNumber.Should().Be(1);
        chapter.GetCurrentVersion().VersionName.Should().Contain("Bản Thảo");
        chapter.GetCurrentVersion().IsAutoSave.Should().BeFalse();
        chapter.GetCurrentVersion().Status.Should().Be(ChapterStatus.Draft);
        chapter.GetCurrentVersion().Title.Should().Be("UpdatedTitle");
        chapter.GetCurrentVersion().Thumbnail.Should().Be("UpdatedThumbnail");
        chapter.GetCurrentVersion().Content.Should().Be("""["img1.png","img2.png","img3.png"]""");
        chapter.GetCurrentVersion().Note.Should().Be("UpdatedNote");
        chapter.PublishedDate.Should().BeNull();
        chapter.LastModified.Should().BeWithin(TimeSpan.FromMilliseconds(100));
        chapter.LastModifiedBy.Should().Be(userId);
    }

    [Fact]
    public async Task AutoSaveComicChapterShouldPersistToDatabase()
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

        int chapterId = await SendAsync(new CreateComicChapterCommand(
            seriesId,
            volumeId,
            "TestTitle",
            "TestThumbnail",
            ["img1.png", "img2.png"],
            "TestNote"
        ));

        UpdateComicChapterCommand updateChapterCommand = new(
            seriesId,
            volumeId,
            chapterId,
            "UpdatedTitle",
            "UpdatedThumbnail",
            ["img1.png", "img2.png", "img3.png"],
            "UpdatedNote",
            true);

        // Act
        await SendAsync(updateChapterCommand);

        // Assert
        IApplicationDbContext context = GetContext();

        Chapter? chapter = context.Chapters.SingleOrDefault(c => c.Id == chapterId);

        chapter!.ChapterNumber.Should().Be(1);
        chapter.GetCurrentVersion().VersionName.Should().Contain("Bản Lưu");
        chapter.GetCurrentVersion().IsAutoSave.Should().BeTrue();
        chapter.GetCurrentVersion().Status.Should().Be(ChapterStatus.Draft);
        chapter.GetCurrentVersion().Title.Should().Be("UpdatedTitle");
        chapter.GetCurrentVersion().Thumbnail.Should().Be("UpdatedThumbnail");
        chapter.GetCurrentVersion().Content.Should().Be("""["img1.png","img2.png","img3.png"]""");
        chapter.GetCurrentVersion().Note.Should().Be("UpdatedNote");
        chapter.PublishedDate.Should().BeNull();
        chapter.LastModified.Should().BeWithin(TimeSpan.FromMilliseconds(100));
        chapter.LastModifiedBy.Should().Be(userId);
    }

    [Fact]
    public async Task UpdateComicChapterShouldRequireExistingSeries()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        UpdateComicChapterCommand updateComicChapterCommand = new(
            1,
            1,
            1,
            "TestTitle",
            "TestThumbnail",
            ["img1.png", "img2.png"],
            "TestNote"
        );

        // Act
        await FluentActions.Invoking(() => SendAsync(updateComicChapterCommand)).Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateComicChapterShouldRequireValidUser()
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

        UpdateComicChapterCommand updateComicChapterCommand = new(
            seriesId,
            1,
            1,
            "TestTitle",
            "TestThumbnail",
            ["img1.png", "img2.png"],
            "TestNote"
        );

        await RunAsUserAsync("AnotherGuy", "Itsapassword1!", []);

        // Act
        await FluentActions.Invoking(() => SendAsync(updateComicChapterCommand)).Should()
            .ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task UpdateComicChapterShouldRequireSeriesOfTypeComic()
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

        UpdateComicChapterCommand updateComicChapterCommand = new(
            seriesId,
            1,
            1,
            "TestTitle",
            "TestThumbnail",
            ["img1.png", "img2.png"],
            "TestNote"
        );

        // Act
        await FluentActions.Invoking(() => SendAsync(updateComicChapterCommand)).Should()
            .ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateComicChapterShouldRequireExistingVolume()
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

        UpdateComicChapterCommand updateComicChapterCommand = new(
            seriesId,
            1,
            1,
            "TestTitle",
            "TestThumbnail",
            ["img1.png", "img2.png"],
            "TestNote"
        );

        // Act
        await FluentActions.Invoking(() => SendAsync(updateComicChapterCommand)).Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateNovelChapterShouldRequireExistingChapter()
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

        UpdateComicChapterCommand updateComicChapterCommand = new(
            seriesId,
            volumeId,
            1,
            "TestTitle",
            "TestThumbnail",
            ["img1.png", "img2.png"],
            "TestNote"
        );

        // Act
        await FluentActions.Invoking(() => SendAsync(updateComicChapterCommand)).Should()
            .ThrowAsync<NotFoundException>();
    }
}
