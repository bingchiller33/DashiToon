using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateComicChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Queries.PreviewComicChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.AuthorStudio.Chapters.Queries;

using static Testing;

[Collection("Serialize")]
public class PreviewComicChapterTest : BaseIntegrationTest
{
    public PreviewComicChapterTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task PreviewComicChapterShouldSuccess()
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

        Chapter? chapter = await FindAsync<Chapter>(chapterId);

        // Act
        ComicChapterPreviewVm preview =
            await SendAsync(new PreviewComicChapterQuery(seriesId, volumeId, chapterId, chapter!.CurrentVersionId));

        // Assert
        preview.Should().NotBeNull();
        preview.ChapterId.Should().Be(chapterId);
        preview.VersionId.Should().Be(chapter.CurrentVersionId);
        preview.VersionName.Should().Contain("Bản Thảo");
        preview.Title.Should().Be("TestTitle1");
        preview.Thumbnail.Should().Be("TestThumbnail1");
        preview.Content.Should().HaveCount(2);
        preview.Note.Should().Be("TestNote1");
    }

    [Fact]
    public async Task PreviewComicChapterShouldRequireExistingSeries()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new PreviewComicChapterQuery(1, 1, 1, Guid.Empty)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task PreviewComicChapterShouldOnlyAllowAuthor()
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
        await FluentActions.Invoking(() => SendAsync(new PreviewComicChapterQuery(seriesId, 1, 1, Guid.Empty)))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task PreviewComicChapterShouldRequireSeriesOfTypeComic()
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
        await FluentActions.Invoking(() => SendAsync(new PreviewComicChapterQuery(seriesId, 1, 1, Guid.Empty)))
            .Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task PreviewComicChapterShouldRequireExistingVolume()
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
        await FluentActions.Invoking(() => SendAsync(new PreviewComicChapterQuery(seriesId, 1, 1, Guid.Empty)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task PreviewComicChapterShouldRequireExistingChapter()
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
        await FluentActions.Invoking(() => SendAsync(new PreviewComicChapterQuery(seriesId, volumeId, 1, Guid.Empty)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task PreviewComicChapterShouldRequireExistingChapterVersion()
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

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new PreviewComicChapterQuery(
                seriesId,
                volumeId,
                chapterId,
                Guid.Empty)))
            .Should().ThrowAsync<NotFoundException>();
    }
}
