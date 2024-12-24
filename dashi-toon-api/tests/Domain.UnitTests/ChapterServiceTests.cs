using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using DashiToon.Api.Domain.Exceptions;
using DashiToon.Api.Domain.Services;
using DashiToon.Api.Domain.ValueObjects;
using DashiToon.Api.Infrastructure.Identity;
using FluentAssertions;

namespace Domain.UnitTests;

public class ChapterServiceTests
{
    [Fact]
    public void UnlockChapterShouldRequirePublishedChapter()
    {
        // Arrange
        ApplicationUser? user = new();

        Chapter? chapter = Chapter.Create(
            "Test Chapter",
            "Thumbnail",
            "Test content",
            null
        );

        ChapterService? chapterService = new();

        // Act & Assert
        FluentActions.Invoking(() => chapterService.UnlockChapter(user, chapter))
            .Should().Throw<NotPublishedChapterCannotBeUnlockedException>();
    }

    [Fact]
    public void UnlockChapterShouldNotAllowedUnlockingAdvanceChapter()
    {
        // Arrange
        ApplicationUser? user = new();

        Chapter? chapter = Chapter.Create(
            "Test Chapter",
            "Thumbnail",
            "Test content",
            null
        );
        chapter.SchedulePublish(DateTimeOffset.UtcNow.AddDays(1));

        ChapterService? chapterService = new();
        // Act & Assert
        FluentActions.Invoking(() => chapterService.UnlockChapter(user, chapter))
            .Should().Throw<AdvanceChapterCannotBeUnlockedException>();
    }

    [Fact]
    public void UnlockChapterShouldRequireSufficientUserKanaBalance()
    {
        // Arrange
        ApplicationUser? user = new();

        Chapter? chapter = Chapter.Create(
            "Test Chapter",
            "Thumbnail",
            "Test content",
            null
        );
        chapter.SetPrice(30);
        chapter.PublishImmediately();

        ChapterService? chapterService = new();

        // Act & Assert
        FluentActions.Invoking(() => chapterService.UnlockChapter(user, chapter))
            .Should().Throw<BalanceInsufficientException>();
    }

    [Fact]
    public void UnlockChapterShouldNotAllowedUnlockingOwnedChapters()
    {
        // Arrange
        ApplicationUser? user = new();

        Chapter? chapter = Chapter.Create(
            "Test Chapter",
            "Thumbnail",
            "Test content",
            null
        );
        chapter.PublishImmediately();
        user.UnlockChapter(chapter);

        ChapterService? chapterService = new();

        // Act & Assert
        FluentActions.Invoking(() => chapterService.UnlockChapter(user, chapter))
            .Should().Throw<ChapterAlreadyUnlockedException>();
    }

    [Fact]
    public void UnlockChapterShouldPrioritizeUnlockingByKanaCoin()
    {
        // Arrange
        ApplicationUser? user = new();

        MissionService.DailyCheckin(user);

        KanaGoldPack? kanaGoldPack = KanaGoldPack.Create(1000, Price.CreateNew(10_000, "VND"), true);
        PurchaseOrder? purchaseOrder = KanaService.CreatePurchaseOrder("order", user, kanaGoldPack);
        KanaService.CompleteOrder(user, purchaseOrder, kanaGoldPack);

        Chapter? chapter = Chapter.Create(
            "Test Chapter",
            "Thumbnail",
            "Test content",
            null
        );
        chapter.Id = 1;
        chapter.SetPrice(30);
        chapter.PublishImmediately();

        ChapterService? chapterService = new();

        // Act
        chapterService.UnlockChapter(user, chapter);

        // Assert
        user.KanaCoin.Should().Be(70);
        user.Ledgers.Last().Currency.Should().Be(KanaType.Coin);
        user.Ledgers.Last().Type.Should().Be(TransactionType.Spend);
        user.Ledgers.Last().Amount.Should().Be(-30);
        user.Ledgers.Last().ChapterId.Should().Be(1);
    }

    [Fact]
    public void UnlockChapterShouldUnlockByKanaGold()
    {
        // Arrange
        ApplicationUser? user = new();

        KanaGoldPack? kanaGoldPack = KanaGoldPack.Create(1000, Price.CreateNew(10_000, "VND"), true);
        PurchaseOrder? purchaseOrder = KanaService.CreatePurchaseOrder("order", user, kanaGoldPack);
        KanaService.CompleteOrder(user, purchaseOrder, kanaGoldPack);

        Chapter? chapter = Chapter.Create(
            "Test Chapter",
            "Thumbnail",
            "Test content",
            null
        );
        chapter.Id = 1;
        chapter.SetPrice(30);
        chapter.PublishImmediately();

        ChapterService? chapterService = new();

        // Act
        chapterService.UnlockChapter(user, chapter);

        // Assert
        user.KanaGold.Should().Be(970);
        user.Ledgers.Last().Currency.Should().Be(KanaType.Gold);
        user.Ledgers.Last().Type.Should().Be(TransactionType.Spend);
        user.Ledgers.Last().Amount.Should().Be(-30);
        user.Ledgers.Last().ChapterId.Should().Be(1);
    }

    [Fact]
    public void IsGuestUserAllowedToReadChapterShouldRequireExistingChapter()
    {
        // Arrange
        Chapter? chapter = Chapter.Create(
            "Test Chapter",
            "Thumbnail",
            "Test content",
            null
        );

        chapter.Id = 1;
        chapter.PublishImmediately();

        List<Chapter>? chapters = new() { chapter };

        ChapterService? chapterService = new();
        // Act & Assert
        FluentActions.Invoking(() => chapterService.IsGuestUserAllowedToReadChapter(chapters, 2))
            .Should().Throw<ChapterNotFoundException>();
    }

    [Fact]
    public void IsGuestUserAllowedToReadChapterShouldNotAllowedForAdvanceChapter()
    {
        // Arrange
        Chapter? chapter = Chapter.Create(
            "Test Chapter",
            "Thumbnail",
            "Test content",
            null
        );

        chapter.Id = 1;
        chapter.SchedulePublish(DateTimeOffset.UtcNow.AddDays(1));

        List<Chapter>? chapters = new() { chapter };

        ChapterService? chapterService = new();
        // Act & Assert
        bool isAllowed = chapterService.IsGuestUserAllowedToReadChapter(chapters, 1);
        isAllowed.Should().BeFalse();
    }

    [Fact]
    public void IsGuestUserAllowedToReadChapterShouldNotAllowedForPaidChapter()
    {
        // Arrange
        Chapter? chapter = Chapter.Create(
            "Test Chapter",
            "Thumbnail",
            "Test content",
            null
        );

        chapter.Id = 1;
        chapter.SetPrice(30);
        chapter.PublishImmediately();

        List<Chapter>? chapters = new() { chapter };

        ChapterService? chapterService = new();

        // Act & Assert
        bool isAllowed = chapterService.IsGuestUserAllowedToReadChapter(chapters, 1);
        isAllowed.Should().BeFalse();
    }

    [Fact]
    public void IsGuestUserAllowedToReadChapterShouldAllowedForFreeChapter()
    {
        // Arrange
        Chapter? chapter = Chapter.Create(
            "Test Chapter",
            "Thumbnail",
            "Test content",
            null
        );

        chapter.Id = 1;
        chapter.PublishImmediately();

        List<Chapter>? chapters = new() { chapter };

        ChapterService? chapterService = new();

        // Act & Assert
        bool isAllowed = chapterService.IsGuestUserAllowedToReadChapter(
            chapters,
            1
        );
        isAllowed.Should().BeTrue();
    }
}
