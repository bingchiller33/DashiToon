using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateComicChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.PublishChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Notifications.Commands.MarkAllAsRead;
using DashiToon.Api.Application.Notifications.Commands.MarkAsRead;
using DashiToon.Api.Application.Notifications.Queries.GetUserNotifications;
using DashiToon.Api.Application.Users.Commands.FollowSeries;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.Notifications.Commands;

using static Testing;

[Collection("Serialize")]
public class MarkAsReadTest : BaseIntegrationTest
{
    public MarkAsReadTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task MarkAsReadShouldMarkNotificationsAsRead()
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

        await SendAsync(new FollowSeriesCommand(seriesId));

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "TestName", "TestIntroduction"));

        int chapterId1 = await SendAsync(new CreateComicChapterCommand(
            seriesId,
            volumeId,
            "TestTitle",
            "TestThumbnail",
            ["image1.png", "image2.png"],
            "TestNote"
        ));

        await SendAsync(new PublishChapterCommand(seriesId, volumeId, chapterId1));

        PaginatedList<NotificationVm>? notifications = await SendAsync(new GetUserNotificationsQuery());

        // Act
        await SendAsync(new MarkAsReadCommand(notifications.Items.First().Id));

        // Assert
        PaginatedList<NotificationVm>? updatedNotifications = await SendAsync(new GetUserNotificationsQuery());

        updatedNotifications.Items.First().IsRead.Should().BeTrue();
    }
}
