using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Infrastructure.Identity;
using FluentAssertions;

namespace Domain.UnitTests;

public class NotificationTests
{
    [Fact]
    public void CreateNotificationShouldCreateSuccessfully()
    {
        // Arrange
        ApplicationUser? recipientUser = new();
        string? title = "Test Title";
        string? content = "Test Content";
        int chapterId = 1;

        // Act
        Notification? notification = Notification.Create(recipientUser.Id, title, content, chapterId);

        // Assert
        notification.UserId.Should().Be(recipientUser.Id);
        notification.Title.Should().Be(title);
        notification.Content.Should().Be(content);
        notification.IsRead.Should().Be(false);
        notification.Timestamp.Should().BeWithin(TimeSpan.FromMilliseconds(100));
        notification.ChapterId.Should().Be(chapterId);
    }

    [Fact]
    public void MarkAsReadShouldUpdateSuccessfully()
    {
        // Arrange
        ApplicationUser? recipientUser = new();
        string? title = "Test Title";
        string? content = "Test Content";
        int chapterId = 1;

        Notification? notification = Notification.Create(recipientUser.Id, title, content, chapterId);

        // Act
        notification.MarkAsRead();

        // Assert
        notification.IsRead.Should().Be(true);
    }
}
