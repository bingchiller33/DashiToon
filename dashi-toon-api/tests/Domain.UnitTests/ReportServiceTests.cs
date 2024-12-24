using DashiToon.Api.Domain.Services;
using DashiToon.Api.Infrastructure.Identity;
using FluentAssertions;

namespace Domain.UnitTests;

public class ReportServiceTests
{
    [Fact]
    public void MuteUserShouldUpdateMuteUntil()
    {
        // Arrange
        ApplicationUser? user = new();
        int duration = 10;

        DateTimeOffset expectedMuteUntil = DateTimeOffset.UtcNow.AddDays(duration);

        // Act
        ReportService.MuteUser(user, duration);

        // Assert
        user.MuteUntil.Should().BeCloseTo(expectedMuteUntil, TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public void MuteUserShouldRequireValidMuteDurations()
    {
        // Arrange
        ApplicationUser? user = new();
        int duration = 0;

        // Act
        FluentActions.Invoking(() => ReportService.MuteUser(user, duration))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MuteUserShouldStackDurations()
    {
        // Arrange
        ApplicationUser? user = new();
        int duration = 10;

        DateTimeOffset expectedMuteUntil = DateTimeOffset.UtcNow.AddDays(duration * 2);

        // Act
        ReportService.MuteUser(user, duration);
        ReportService.MuteUser(user, duration);

        // Assert
        user.MuteUntil.Should().BeCloseTo(expectedMuteUntil, TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public void RestrictUserShouldUpdateRestrictPublishUntil()
    {
        // Arrange
        ApplicationUser? user = new();
        int duration = 10;

        DateTimeOffset expectedRestrictUntil = DateTimeOffset.UtcNow.AddDays(duration);

        // Act
        ReportService.RestrictUser(user, duration);

        // Assert
        user.RestrictPublishUntil.Should().BeCloseTo(expectedRestrictUntil, TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public void RestrictUserShouldRequireValidRestrictPublishDurations()
    {
        // Arrange
        ApplicationUser? user = new();
        int duration = 0;

        // Act
        FluentActions.Invoking(() => ReportService.RestrictUser(user, duration))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RestrictUserShouldStackDurations()
    {
        // Arrange
        ApplicationUser? user = new();
        int duration = 10;

        DateTimeOffset expectedRestrictUntil = DateTimeOffset.UtcNow.AddDays(duration * 2);

        // Act
        ReportService.RestrictUser(user, duration);
        ReportService.RestrictUser(user, duration);

        // Assert
        user.RestrictPublishUntil.Should().BeCloseTo(expectedRestrictUntil, TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public void MuteUserShouldNotBeAllowedToComment()
    {
        // Arrange
        ApplicationUser? user = new();
        int duration = 10;

        ReportService.MuteUser(user, duration);

        // Act
        bool isAllowedToComment = ReportService.IsUserAllowedToCommentOrReview(user);

        // Assert
        isAllowedToComment.Should().BeFalse();
    }

    [Fact]
    public void UserThatHaveExpiredMuteDurationShouldBeAllowedToComment()
    {
        // Arrange
        ApplicationUser? user = new();

        user.Mute(DateTimeOffset.UtcNow.AddDays(-1));

        // Act
        bool isAllowedToComment = ReportService.IsUserAllowedToCommentOrReview(user);

        // Assert
        isAllowedToComment.Should().BeTrue();
    }

    [Fact]
    public void NewUserShouldBeAllowedToComment()
    {
        // Arrange
        ApplicationUser? user = new();

        // Act
        bool isAllowedToComment = ReportService.IsUserAllowedToCommentOrReview(user);

        // Assert
        isAllowedToComment.Should().BeTrue();
    }

    [Fact]
    public void RestrictUserShouldNotBeAllowedToPublish()
    {
        // Arrange
        ApplicationUser? user = new();
        int duration = 10;

        ReportService.RestrictUser(user, duration);

        // Act
        bool isAllowedToPublish = ReportService.IsUserAllowedToPublish(user);

        // Assert
        isAllowedToPublish.Should().BeFalse();
    }

    [Fact]
    public void UserThatHaveExpiredRestrictDurationShouldBeAllowedToPublish()
    {
        // Arrange
        ApplicationUser? user = new();

        user.RestrictPublish(DateTimeOffset.UtcNow.AddDays(-1));

        // Act
        bool isUserAllowedToPublish = ReportService.IsUserAllowedToPublish(user);

        // Assert
        isUserAllowedToPublish.Should().BeTrue();
    }

    [Fact]
    public void NewUserShouldBeAllowedToPublish()
    {
        // Arrange
        ApplicationUser? user = new();

        // Act
        bool isUserAllowedToPublish = ReportService.IsUserAllowedToPublish(user);

        // Assert
        isUserAllowedToPublish.Should().BeTrue();
    }
}
