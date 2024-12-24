using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Infrastructure.Identity;
using FluentAssertions;

namespace Domain.UnitTests;

public class FollowTests
{
    [Fact]
    public void CreateFollowShouldCreateSuccessfully()
    {
        // Act
        int seriesId = 1;
        ApplicationUser? user = new();

        // Act
        Follow? follow = Follow.CreateNew(seriesId, user.Id);

        // Assert
        follow.UserId.Should().Be(user.Id);
        follow.SeriesId.Should().Be(seriesId);
        follow.IsNotified.Should().BeTrue();
        follow.Timestamp.Should().BeWithin(TimeSpan.FromMilliseconds(100));
        follow.LatestChapterId.Should().BeNull();
    }

    [Fact]
    public void LatestReadShouldUpdateLatestReadChapter()
    {
        // Arrange
        int seriesId = 1;
        ApplicationUser? user = new();

        Follow? follow = Follow.CreateNew(seriesId, user.Id);

        // Act
        follow.LatestRead(10);

        // Assert
        follow.LatestChapterId.Should().Be(10);
    }

    [Fact]
    public void ChangeNotificationStatusShouldUpdateNotificationStatus()
    {
        // Arrange
        int seriesId = 1;
        ApplicationUser? user = new();

        Follow? follow = Follow.CreateNew(seriesId, user.Id);

        // Act
        follow.ChangeNotificationStatus(false);

        // Assert
        follow.IsNotified.Should().BeFalse();
    }
}
