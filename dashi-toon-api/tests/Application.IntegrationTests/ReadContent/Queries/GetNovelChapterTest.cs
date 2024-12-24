using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.PublishChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.SetChapterPricing;
using DashiToon.Api.Application.AuthorStudio.DashiFans.Commands.CreateDashiFan;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.ReadContent.Commands.UnlockChapter;
using DashiToon.Api.Application.ReadContent.Queries.GetNovelChapter;
using DashiToon.Api.Application.Users.Commands.CheckinUser;
using DashiToon.Api.Application.Users.Commands.SubscribeSeries;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.IntegrationTests.ReadContent.Queries;

using static Testing;

[Collection("Serialize")]
public class GetNovelChapterTest : BaseIntegrationTest
{
    public GetNovelChapterTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetNovelChapterShouldAllowGuestUserToReadFreeChapter()
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

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "Vol1", string.Empty));

        int chapterId = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            """<img src="" data-img-name="bruh.png"/>""",
            "TestNote1"
        ));

        await SendAsync(new PublishChapterCommand(seriesId, volumeId, chapterId));

        LogOut();

        // Act
        NovelChapterDetailVm? chapterContent = await SendAsync(new GetNovelChapterQuery(seriesId, volumeId, chapterId));

        // Assert
        chapterContent.Id.Should().Be(chapterId);
        chapterContent.ChapterNumber.Should().Be(1);
        chapterContent.Title.Should().Be("TestTitle1");
        chapterContent.Thumbnail.Should().Be("thumbnails/TestThumbnail1");
        chapterContent.Content.Should()
            .Be(
                """<img src="chapters/bruh.png" data-img-name="bruh.png" data-img-width="0" data-img-height="0" data-img-size="0">""");
        chapterContent.Price.Should().BeNull();
        chapterContent.IsAdvanceChapter.Should().BeFalse();
        chapterContent.Status.Should().Be(ChapterStatus.Published);
        chapterContent.PublishedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task GetNovelChapterShouldAllowUserToReadOwnedChapter()
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

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "Vol1", string.Empty));

        int chapterId = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            """<img src="" data-img-name="bruh.png"/>""",
            "TestNote1"
        ));

        await SendAsync(new SetChapterPricingCommand(seriesId, volumeId, chapterId, 30));
        await SendAsync(new PublishChapterCommand(seriesId, volumeId, chapterId));

        await SendAsync(new CheckinUserCommand());
        await SendAsync(new UnlockChapterCommand(seriesId, volumeId, chapterId));

        // Act
        NovelChapterDetailVm? chapterContent = await SendAsync(new GetNovelChapterQuery(seriesId, volumeId, chapterId));

        // Assert
        chapterContent.Id.Should().Be(chapterId);
        chapterContent.ChapterNumber.Should().Be(1);
        chapterContent.Title.Should().Be("TestTitle1");
        chapterContent.Thumbnail.Should().Be("thumbnails/TestThumbnail1");
        chapterContent.Price.Should().Be(30);
        chapterContent.IsAdvanceChapter.Should().BeFalse();
        chapterContent.Status.Should().Be(ChapterStatus.Published);
        chapterContent.PublishedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task GetNovelChapterShouldAllowSubscribedUserToReadAdvanceChapter()
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

        Guid tierId = await SendAsync(new CreateDashiFanCommand(
            seriesId,
            "TestTier",
            "TestDescription",
            2,
            10_000,
            "VND"));

        int volumeId1 = await SendAsync(new CreateVolumeCommand(seriesId, "Vol1", string.Empty));

        int chapterId1 = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId1,
            "TestTitle1",
            "TestThumbnail1",
            """<img src="" data-img-name="bruh.png"/>""",
            "TestNote1"
        ));
        await SendAsync(new PublishChapterCommand(seriesId, volumeId1, chapterId1, DateTimeOffset.UtcNow.AddDays(1)));

        int chapterId2 = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId1,
            "TestTitle2",
            "TestThumbnail2",
            """<img src="" data-img-name="bruh.png"/>""",
            "TestNote2"
        ));
        await SendAsync(new PublishChapterCommand(seriesId, volumeId1, chapterId2, DateTimeOffset.UtcNow.AddDays(1)));

        await SendAsync(new SubscribeSeriesCommand(seriesId, tierId, string.Empty, string.Empty));

        IApplicationDbContext? context = GetContext();
        Subscription? subscription = await context.Subscriptions.FirstAsync(s =>
            s.DashiFanId == tierId && s.Status == SubscriptionStatus.Pending);
        subscription.Activate(DateTimeOffset.UtcNow.AddDays(30));

        await context.SaveChangesAsync(default);

        // Act & Assert
        NovelChapterDetailVm? chapterContent =
            await SendAsync(new GetNovelChapterQuery(seriesId, volumeId1, chapterId2));
        chapterContent.Id.Should().Be(chapterId2);
        chapterContent.ChapterNumber.Should().Be(2);
        chapterContent.Title.Should().Be("TestTitle2");
        chapterContent.Thumbnail.Should().Be("thumbnails/TestThumbnail2");
        chapterContent.Price.Should().BeNull();
        chapterContent.IsAdvanceChapter.Should().BeTrue();
        chapterContent.Status.Should().Be(ChapterStatus.Published);
        chapterContent.PublishedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task GetNovelChapterShouldRequirePublishedSeries()
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
                new ContentQuestionnaire(ContentCategory.Nudity, 2),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 3),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
            ],
            [],
            [],
            null));

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new GetNovelChapterQuery(seriesId, 1, 1)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetNovelChapterShouldRequireSeriesOfTypeNovel()
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

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new GetNovelChapterQuery(seriesId, 1, 1)))
            .Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task GetNovelChapterShouldRequireExistingVolume()
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

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new GetNovelChapterQuery(seriesId, 1, 1)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetNovelChapterShouldRequirePublishedChapter()
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

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "Vol1", string.Empty));

        int chapterId = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            """<img src="" data-img-name="bruh.png"/>""",
            "TestNote1"
        ));

        // Act
        await FluentActions.Invoking(() => SendAsync(new GetNovelChapterQuery(seriesId, volumeId, chapterId)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetNovelChapterShouldRequireAuthorizedUserToReadPaidChapter()
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

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "Vol1", string.Empty));

        int chapterId = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            """<img src="" data-img-name="bruh.png"/>""",
            "TestNote1"
        ));

        await SendAsync(new SetChapterPricingCommand(seriesId, volumeId, chapterId, 30));
        await SendAsync(new PublishChapterCommand(seriesId, volumeId, chapterId));

        LogOut();

        // Act
        await FluentActions.Invoking(() => SendAsync(new GetNovelChapterQuery(seriesId, volumeId, chapterId)))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetNovelChapterShouldRequireUserToUnlockChapterToReadPaidChapter()
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

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "Vol1", string.Empty));

        int chapterId = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            """<img src="" data-img-name="bruh.png"/>""",
            "TestNote1"
        ));

        await SendAsync(new SetChapterPricingCommand(seriesId, volumeId, chapterId, 30));
        await SendAsync(new PublishChapterCommand(seriesId, volumeId, chapterId));

        // Act
        await FluentActions.Invoking(() => SendAsync(new GetNovelChapterQuery(seriesId, volumeId, chapterId)))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task GetNovelChapterShouldRequireUserToSubscribeToReadAdvanceChapter()
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

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "Vol1", string.Empty));

        int chapterId = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            """<img src="" data-img-name="bruh.png"/>""",
            "TestNote1"
        ));

        await SendAsync(new SetChapterPricingCommand(seriesId, volumeId, chapterId, 30));
        await SendAsync(new PublishChapterCommand(seriesId, volumeId, chapterId, DateTimeOffset.UtcNow.AddDays(1)));

        // Act
        await FluentActions.Invoking(() => SendAsync(new GetNovelChapterQuery(seriesId, volumeId, chapterId)))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task GetNovelChapterShouldLimitAdvanceChapterForSubscribedUser()
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

        Guid tierId = await SendAsync(new CreateDashiFanCommand(
            seriesId,
            "TestTier",
            "TestDescription",
            2,
            10_000,
            "VND"));

        int volumeId1 = await SendAsync(new CreateVolumeCommand(seriesId, "Vol1", string.Empty));

        int chapterId1 = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId1,
            "TestTitle1",
            "TestThumbnail1",
            """<img src="" data-img-name="bruh.png"/>""",
            "TestNote1"
        ));
        await SendAsync(new PublishChapterCommand(seriesId, volumeId1, chapterId1, DateTimeOffset.UtcNow.AddDays(1)));

        int chapterId2 = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId1,
            "TestTitle2",
            "TestThumbnail2",
            """<img src="" data-img-name="bruh.png"/>""",
            "TestNote2"
        ));
        await SendAsync(new PublishChapterCommand(seriesId, volumeId1, chapterId2, DateTimeOffset.UtcNow.AddDays(1)));

        int volumeId2 = await SendAsync(new CreateVolumeCommand(seriesId, "Vol2", string.Empty));

        int chapterId3 = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId2,
            "TestTitle3",
            "TestThumbnail3",
            """<img src="" data-img-name="bruh.png"/>""",
            "TestNote3"
        ));
        await SendAsync(new PublishChapterCommand(seriesId, volumeId2, chapterId3, DateTimeOffset.UtcNow.AddDays(1)));

        await SendAsync(new SubscribeSeriesCommand(seriesId, tierId, string.Empty, string.Empty));

        IApplicationDbContext? context = GetContext();
        Subscription? subscription = await context.Subscriptions.FirstAsync(s =>
            s.DashiFanId == tierId && s.Status == SubscriptionStatus.Pending);
        subscription.Activate(DateTimeOffset.UtcNow.AddDays(30));

        await context.SaveChangesAsync(default);

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new GetNovelChapterQuery(seriesId, volumeId2, chapterId3)))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }
}
