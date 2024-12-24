using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.PublishChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.SetChapterPricing;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.SeriesDetail.Queries.GetSeriesChapters;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.SeriesDetail.Queries;

using static Testing;

[Collection("Serialize")]
public class GetSeriesChaptersTest : BaseIntegrationTest
{
    public GetSeriesChaptersTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetSeriesChaptersShouldReturnCorrectChapters()
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

        await SendAsync(new UpdateSeriesCommand(
            seriesId,
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesStatus.Ongoing,
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

        int chapterId1 = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            "<p>My life be like ooooh ahhhhhh</p>",
            "TestNote1"));
        await SendAsync(new PublishChapterCommand(seriesId, volumeId, chapterId1));

        int chapterId2 = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId,
            "TestTitle2",
            "TestThumbnail2",
            "<p>My life be like ooooh ahhhhhh</p>",
            "TestNote2"));

        await SendAsync(new PublishChapterCommand(seriesId, volumeId, chapterId2, DateTimeOffset.UtcNow.AddDays(1)));
        await SendAsync(new SetChapterPricingCommand(seriesId, volumeId, chapterId2, 30));

        // Act
        List<SeriesChapterVm> chapters = await SendAsync(new GetSeriesChaptersQuery(seriesId, volumeId));

        // Assert
        chapters.Should().HaveCount(2);
        chapters[0].Id.Should().Be(chapterId2);
        chapters[0].ChapterNumber.Should().Be(2);
        chapters[0].Title.Should().Be("TestTitle2");
        chapters[0].IsAdvanceChapter.Should().Be(true);
        chapters[0].Price.Should().Be(30);

        chapters[1].Id.Should().Be(chapterId1);
        chapters[1].ChapterNumber.Should().Be(1);
        chapters[1].Title.Should().Be("TestTitle1");
        chapters[1].IsAdvanceChapter.Should().Be(false);
        chapters[1].Price.Should().BeNull();
    }

    [Fact]
    public async Task GetSeriesChaptersShouldReturnPublishedChapterOnly()
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

        await SendAsync(new UpdateSeriesCommand(
            seriesId,
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesStatus.Ongoing,
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

        int chapterId1 = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            "<p>My life be like ooooh ahhhhhh</p>",
            "TestNote1"));

        int chapterId2 = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId,
            "TestTitle2",
            "TestThumbnail2",
            "<p>My life be like ooooh ahhhhhh</p>",
            "TestNote2"));

        await SendAsync(new PublishChapterCommand(seriesId, volumeId, chapterId1));

        // Act
        List<SeriesChapterVm> chapters = await SendAsync(new GetSeriesChaptersQuery(seriesId, volumeId));

        // Assert
        chapters.Should().HaveCount(1);

        chapters[0].Id.Should().Be(chapterId1);
        chapters[0].ChapterNumber.Should().Be(1);
        chapters[0].Title.Should().Be("TestTitle1");
    }

    [Fact]
    public async Task GetSeriesDetailShouldRequireExistingSeries()
    {
        // Act
        List<SeriesChapterVm> chapters = await SendAsync(new GetSeriesChaptersQuery(1, 1));

        // Assert
        chapters.Should().BeEmpty();
    }

    [Theory]
    [InlineData(SeriesStatus.Hiatus)]
    [InlineData(SeriesStatus.Trashed)]
    [InlineData(SeriesStatus.Draft)]
    public async Task GetSeriesChaptersShouldRequireValidSeriesStatus(SeriesStatus status)
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
                new ContentQuestionnaire(ContentCategory.Nudity, 2),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 3),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
            ],
            [],
            [],
            null));

        await SendAsync(new UpdateSeriesCommand(
            seriesId,
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            status,
            [1, 2, 3],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 2),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 3),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
            ],
            [],
            [],
            null));

        // Act
        List<SeriesChapterVm> chapters = await SendAsync(new GetSeriesChaptersQuery(seriesId, 1));

        // Assert
        chapters.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSeriesChaptersShouldRequireExistingVolumes()
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

        await SendAsync(new UpdateSeriesCommand(
            seriesId,
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesStatus.Ongoing,
            [1, 2, 3],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 2),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 3),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
            ],
            [],
            [],
            null));

        // Act
        List<SeriesChapterVm> chapters = await SendAsync(new GetSeriesChaptersQuery(seriesId, 1));

        // Assert
        chapters.Should().BeEmpty();
    }
}
