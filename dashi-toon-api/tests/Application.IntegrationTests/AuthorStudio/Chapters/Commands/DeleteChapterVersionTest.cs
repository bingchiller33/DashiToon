using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateComicChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.DeleteChapterVersion;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.UpdateComicChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.IntegrationTests.AuthorStudio.Chapters.Commands;

using static Testing;

[Collection("Serialize")]
public class DeleteChapterVersionTest : BaseIntegrationTest
{
    public DeleteChapterVersionTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task DeleteChapterVersionShouldRemoveChapterVersion()
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

        await SendAsync(new UpdateComicChapterCommand(
            seriesId,
            volumeId,
            chapterId,
            "TestTitle",
            "TestThumbnail",
            ["image1.png", "image2.png"],
            "TestNote"
        ));

        // Act
        await SendAsync(new DeleteChapterVersionCommand(seriesId, volumeId, chapterId, version));

        // Assert
        IApplicationDbContext context = GetContext();

        Chapter? chapter = await context.Chapters.FirstOrDefaultAsync(c => c.Id == chapterId);

        chapter.Should().NotBeNull();
        chapter!.Versions.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteChapterVersionShouldRequireExistingSeries()
    {
        // AAA
        await RunAsDefaultUserAsync();

        await FluentActions
            .Invoking(() => SendAsync(new DeleteChapterVersionCommand(1, 1, 1, Guid.NewGuid())))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteChapterVersionShouldRequireExistingVolume()
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
            .Invoking(() => SendAsync(new DeleteChapterVersionCommand(seriesId, 1, 1, Guid.NewGuid())))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteChapterVersionShouldRequireExistingChapter()
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
                SendAsync(new DeleteChapterVersionCommand(seriesId, volumeId, 1, Guid.NewGuid())))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteChapterVersionShouldAllowOnlyAuthorToModify()
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

        await FluentActions.Invoking(() =>
                SendAsync(new DeleteChapterVersionCommand(seriesId, volumeId, 1, Guid.NewGuid())))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }
}
