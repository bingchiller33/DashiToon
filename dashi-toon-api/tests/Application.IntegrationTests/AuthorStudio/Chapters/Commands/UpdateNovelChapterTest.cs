using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.UpdateNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.AuthorStudio.Chapters.Commands;

using static Testing;

[Collection("Serialize")]
public class UpdateNovelChapterTest : BaseIntegrationTest
{
    public UpdateNovelChapterTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task UpdateNovelChapterShouldPersistToDatabase()
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

        int chapterId = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId,
            "TestTitle",
            "TestThumbnail",
            "<p>My life be like ooooh ahhhhhh</p>",
            "TestNote"
        ));

        UpdateNovelChapterCommand updateChapterCommand = new(
            seriesId,
            volumeId,
            chapterId,
            "UpdatedTitle",
            "UpdatedThumbnail",
            "<p>Hehe boi</p>",
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
        chapter.GetCurrentVersion().Content.Should().Be("<p>Hehe boi</p>");
        chapter.GetCurrentVersion().Note.Should().Be("UpdatedNote");
        chapter.PublishedDate.Should().BeNull();
        chapter.LastModified.Should().BeWithin(TimeSpan.FromMilliseconds(100));
        chapter.LastModifiedBy.Should().Be(userId);
    }

    [Fact]
    public async Task AutoSaveNovelChapterShouldPersistToDatabase()
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

        int chapterId = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId,
            "TestTitle",
            "TestThumbnail",
            "<p>My life be like ooooh ahhhhhh</p>",
            "TestNote"
        ));

        UpdateNovelChapterCommand updateChapterCommand = new(
            seriesId,
            volumeId,
            chapterId,
            "UpdatedTitle",
            "UpdatedThumbnail",
            "<p>Hehe boi</p>",
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
        chapter.GetCurrentVersion().Content.Should().Be("<p>Hehe boi</p>");
        chapter.GetCurrentVersion().Note.Should().Be("UpdatedNote");
        chapter.PublishedDate.Should().BeNull();
        chapter.LastModified.Should().BeWithin(TimeSpan.FromMilliseconds(100));
        chapter.LastModifiedBy.Should().Be(userId);
    }

    [Fact]
    public async Task UpdateNovelChapterShouldRequireExistingSeries()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        UpdateNovelChapterCommand updateNovelChapterCommand = new(
            1,
            1,
            1,
            "TestTitle",
            "TestThumbnail",
            "<p>My life be like ooooh ahhhhhh</p>",
            "TestNote"
        );

        // Act
        await FluentActions.Invoking(() => SendAsync(updateNovelChapterCommand)).Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateNovelChapterShouldRequireValidUser()
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

        UpdateNovelChapterCommand updateNovelChapterCommand = new(
            seriesId,
            1,
            1,
            "TestTitle",
            "TestThumbnail",
            "<p>My life be like ooooh ahhhhhh</p>",
            "TestNote"
        );

        await RunAsUserAsync("AnotherGuy", "Itsapassword1!", []);

        // Act
        await FluentActions.Invoking(() => SendAsync(updateNovelChapterCommand)).Should()
            .ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task UpdateNovelChapterShouldRequireSeriesOfTypeNovel()
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

        UpdateNovelChapterCommand updateNovelChapterCommand = new(
            seriesId,
            1,
            1,
            "TestTitle",
            "TestThumbnail",
            "<p>My life be like ooooh ahhhhhh</p>",
            "TestNote"
        );

        // Act
        await FluentActions.Invoking(() => SendAsync(updateNovelChapterCommand)).Should()
            .ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateNovelChapterShouldRequireExistingVolume()
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

        UpdateNovelChapterCommand updateNovelChapterCommand = new(
            seriesId,
            1,
            1,
            "TestTitle",
            "TestThumbnail",
            "<p>My life be like ooooh ahhhhhh</p>",
            "TestNote"
        );

        // Act
        await FluentActions.Invoking(() => SendAsync(updateNovelChapterCommand)).Should()
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

        UpdateNovelChapterCommand updateNovelChapterCommand = new(
            seriesId,
            volumeId,
            1,
            "TestTitle",
            "TestThumbnail",
            "<p>My life be like ooooh ahhhhhh</p>",
            "TestNote"
        );

        // Act
        await FluentActions.Invoking(() => SendAsync(updateNovelChapterCommand)).Should()
            .ThrowAsync<NotFoundException>();
    }
}
