using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateComicChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.PublishChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.SetChapterPricing;
using DashiToon.Api.Application.AuthorStudio.DashiFans.Commands.CreateDashiFan;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.ReadContent.Commands.UnlockChapter;
using DashiToon.Api.Application.ReadContent.Queries.GetComicChapter;
using DashiToon.Api.Application.Users.Commands.CheckinUser;
using DashiToon.Api.Application.Users.Commands.SubscribeSeries;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.IntegrationTests.ReadContent.Queries;

using static Testing;

[Collection("Serialize")]
public class GetComicChapterTest : BaseIntegrationTest
{
    public GetComicChapterTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetComicChapterShouldAllowGuestUserToReadFreeChapter()
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

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "Vol1", string.Empty));

        int chapterId = await SendAsync(new CreateComicChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            ["image1.png", "image2.png"],
            "TestNote1"
        ));

        await SendAsync(new PublishChapterCommand(seriesId, volumeId, chapterId));

        LogOut();

        // Act
        ComicChapterDetailVm? chapterContent = await SendAsync(new GetComicChapterQuery(seriesId, volumeId, chapterId));

        // Assert
        chapterContent.Id.Should().Be(chapterId);
        chapterContent.ChapterNumber.Should().Be(1);
        chapterContent.Title.Should().Be("TestTitle1");
        chapterContent.Thumbnail.Should().Be("thumbnails/TestThumbnail1");
        chapterContent.Content.Should().HaveCount(2);
        chapterContent.Price.Should().BeNull();
        chapterContent.IsAdvanceChapter.Should().BeFalse();
        chapterContent.Status.Should().Be(ChapterStatus.Published);
        chapterContent.PublishedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task GetComicChapterShouldAllowUserToReadOwnedChapter()
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

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "Vol1", string.Empty));

        int chapterId = await SendAsync(new CreateComicChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            ["image1.png", "image2.png"],
            "TestNote1"
        ));

        await SendAsync(new SetChapterPricingCommand(seriesId, volumeId, chapterId, 30));
        await SendAsync(new PublishChapterCommand(seriesId, volumeId, chapterId));

        await SendAsync(new CheckinUserCommand());
        await SendAsync(new UnlockChapterCommand(seriesId, volumeId, chapterId));

        // Act
        ComicChapterDetailVm? chapterContent = await SendAsync(new GetComicChapterQuery(seriesId, volumeId, chapterId));

        // Assert
        chapterContent.Id.Should().Be(chapterId);
        chapterContent.ChapterNumber.Should().Be(1);
        chapterContent.Title.Should().Be("TestTitle1");
        chapterContent.Thumbnail.Should().Be("thumbnails/TestThumbnail1");
        chapterContent.Content.Should().HaveCount(2);
        chapterContent.Price.Should().Be(30);
        chapterContent.IsAdvanceChapter.Should().BeFalse();
        chapterContent.Status.Should().Be(ChapterStatus.Published);
        chapterContent.PublishedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task GetComicChapterShouldAllowSubscribedUserToReadAdvanceChapter()
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

        Guid tierId = await SendAsync(new CreateDashiFanCommand(
            seriesId,
            "TestTier",
            "TestDescription",
            2,
            10_000,
            "VND"));

        int volumeId1 = await SendAsync(new CreateVolumeCommand(seriesId, "Vol1", string.Empty));

        int chapterId1 = await SendAsync(new CreateComicChapterCommand(
            seriesId,
            volumeId1,
            "TestTitle1",
            "TestThumbnail1",
            ["image1.png", "image2.png"],
            "TestNote1"
        ));
        await SendAsync(new PublishChapterCommand(seriesId, volumeId1, chapterId1, DateTimeOffset.UtcNow.AddDays(1)));

        int chapterId2 = await SendAsync(new CreateComicChapterCommand(
            seriesId,
            volumeId1,
            "TestTitle2",
            "TestThumbnail2",
            ["image1.png", "image2.png"],
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
        ComicChapterDetailVm? chapterContent =
            await SendAsync(new GetComicChapterQuery(seriesId, volumeId1, chapterId2));
        chapterContent.Id.Should().Be(chapterId2);
        chapterContent.ChapterNumber.Should().Be(2);
        chapterContent.Title.Should().Be("TestTitle2");
        chapterContent.Thumbnail.Should().Be("thumbnails/TestThumbnail2");
        chapterContent.Content.Should().HaveCount(2);
        chapterContent.Price.Should().BeNull();
        chapterContent.IsAdvanceChapter.Should().BeTrue();
        chapterContent.Status.Should().Be(ChapterStatus.Published);
        chapterContent.PublishedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task GetComicChapterShouldRequirePublishedSeries()
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

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new GetComicChapterQuery(seriesId, 1, 1)))
            .Should().ThrowAsync<NotFoundException>();
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
        await FluentActions.Invoking(() => SendAsync(new GetComicChapterQuery(seriesId, 1, 1)))
            .Should().ThrowAsync<ValidationException>();
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
        await FluentActions.Invoking(() => SendAsync(new GetComicChapterQuery(seriesId, 1, 1)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetComicChapterShouldRequirePublishedChapter()
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

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "Vol1", string.Empty));

        int chapterId = await SendAsync(new CreateComicChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            ["image1.png", "image2.png"],
            "TestNote1"
        ));

        // Act
        await FluentActions.Invoking(() => SendAsync(new GetComicChapterQuery(seriesId, volumeId, chapterId)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetComicChapterShouldRequireAuthorizedUserToReadPaidChapter()
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

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "Vol1", string.Empty));

        int chapterId = await SendAsync(new CreateComicChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            ["image1.png", "image2.png"],
            "TestNote1"
        ));

        await SendAsync(new SetChapterPricingCommand(seriesId, volumeId, chapterId, 30));
        await SendAsync(new PublishChapterCommand(seriesId, volumeId, chapterId));

        LogOut();

        // Act
        await FluentActions.Invoking(() => SendAsync(new GetComicChapterQuery(seriesId, volumeId, chapterId)))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetComicChapterShouldRequireUserToUnlockChapterToReadPaidChapter()
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

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "Vol1", string.Empty));

        int chapterId = await SendAsync(new CreateComicChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            ["image1.png", "image2.png"],
            "TestNote1"
        ));

        await SendAsync(new SetChapterPricingCommand(seriesId, volumeId, chapterId, 30));
        await SendAsync(new PublishChapterCommand(seriesId, volumeId, chapterId));

        // Act
        await FluentActions.Invoking(() => SendAsync(new GetComicChapterQuery(seriesId, volumeId, chapterId)))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task GetComicChapterShouldRequireUserToSubscribeToReadAdvanceChapter()
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

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "Vol1", string.Empty));

        int chapterId = await SendAsync(new CreateComicChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            ["image1.png", "image2.png"],
            "TestNote1"
        ));

        await SendAsync(new SetChapterPricingCommand(seriesId, volumeId, chapterId, 30));
        await SendAsync(new PublishChapterCommand(seriesId, volumeId, chapterId, DateTimeOffset.UtcNow.AddDays(1)));

        // Act
        await FluentActions.Invoking(() => SendAsync(new GetComicChapterQuery(seriesId, volumeId, chapterId)))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task GetComicChapterShouldLimitAdvanceChapterForSubscribedUser()
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

        Guid tierId = await SendAsync(new CreateDashiFanCommand(
            seriesId,
            "TestTier",
            "TestDescription",
            2,
            10_000,
            "VND"));

        int volumeId1 = await SendAsync(new CreateVolumeCommand(seriesId, "Vol1", string.Empty));

        int chapterId1 = await SendAsync(new CreateComicChapterCommand(
            seriesId,
            volumeId1,
            "TestTitle1",
            "TestThumbnail1",
            ["image1.png", "image2.png"],
            "TestNote1"
        ));
        await SendAsync(new PublishChapterCommand(seriesId, volumeId1, chapterId1, DateTimeOffset.UtcNow.AddDays(1)));

        int chapterId2 = await SendAsync(new CreateComicChapterCommand(
            seriesId,
            volumeId1,
            "TestTitle2",
            "TestThumbnail2",
            ["image1.png", "image2.png"],
            "TestNote2"
        ));
        await SendAsync(new PublishChapterCommand(seriesId, volumeId1, chapterId2, DateTimeOffset.UtcNow.AddDays(1)));

        int volumeId2 = await SendAsync(new CreateVolumeCommand(seriesId, "Vol2", string.Empty));

        int chapterId3 = await SendAsync(new CreateComicChapterCommand(
            seriesId,
            volumeId2,
            "TestTitle3",
            "TestThumbnail3",
            ["image1.png", "image2.png"],
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
        await FluentActions.Invoking(() => SendAsync(new GetComicChapterQuery(seriesId, volumeId2, chapterId3)))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }
}
