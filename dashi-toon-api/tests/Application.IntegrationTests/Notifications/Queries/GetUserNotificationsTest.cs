using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateComicChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.PublishChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Moderation.Commands.ReportSeries;
using DashiToon.Api.Application.Moderation.Commands.ResolveReviewReport;
using DashiToon.Api.Application.Moderation.Commands.ResolveSeriesReport;
using DashiToon.Api.Application.Notifications.Queries.GetUserNotifications;
using DashiToon.Api.Application.Reviews.Commands.WriteSeriesReviews;
using DashiToon.Api.Application.Reviews.Models;
using DashiToon.Api.Application.Users.Commands.FollowSeries;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.Notifications.Queries;

using static Testing;

[Collection("Serialize")]
public class GetUserNotificationsTest : BaseIntegrationTest
{
    public GetUserNotificationsTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task FollowUserShouldGetPublishedChapterNotifications()
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

        int chapterId = await SendAsync(new CreateComicChapterCommand(
            seriesId,
            volumeId,
            "TestTitle",
            "TestThumbnail",
            ["image1.png", "image2.png"],
            "TestNote"
        ));

        // Act
        await SendAsync(new PublishChapterCommand(seriesId, volumeId, chapterId));

        // Assert
        PaginatedList<NotificationVm>? notifications = await SendAsync(new GetUserNotificationsQuery());

        notifications.Items.First().Title.Should().Be("TestTitle đã có chương mới");
        notifications.Items.First().IsRead.Should().BeFalse();
        notifications.Items.First().ChapterId.Should().Be(chapterId);
        notifications.Items.First().VolumeId.Should().Be(volumeId);
        notifications.Items.First().SeriesId.Should().Be(seriesId);
        notifications.Items.First().Type.Should().Be(SeriesType.Comic);
    }

    [Fact]
    public async Task UserShouldGetMuteNotificationsWhenUserIsMuted()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

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
            ["Auth 1"],
            null));

        ReviewVm? review = await SendAsync(new WriteSeriesReviewCommand(
            seriesId,
            """
            Nice Content Nice Content Nice Content Nice Content  
            Nice Content Nice Content Nice Content Nice Content  
            Nice Content Nice Content Nice Content Nice Content  
            Nice Content Nice Content Nice Content Nice Content  
            Nice Content Nice Content Nice Content Nice Content  
            """,
            true));

        await RunAsModeratorAsync();

        // Act
        await SendAsync(new ResolveReviewReportCommand(review.Id, 7));

        // Assert
        await RunAsDefaultUserAsync();

        PaginatedList<NotificationVm>? notifications = await SendAsync(new GetUserNotificationsQuery());

        notifications.Items.First().Title.Should().Be("Vi phạm tiêu chuẩn cộng đồng");
        notifications.Items.First().Content.Should()
            .Be(
                $"Do vi phạm tiêu chuẩn cộng đồng. Bạn sẽ không thể bình luận hoặc đánh giá truyện cho đến hết ngày {DateTimeOffset.UtcNow.AddDays(7):d}");
        notifications.Items.First().IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task UserShouldGetRestrictNotificationsWhenUserIsRestricted()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

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
            ["Auth 1"],
            null));

        await SendAsync(new ReportSeriesCommand(seriesId, "Test Reason"));

        await RunAsModeratorAsync();
        // Act
        await SendAsync(new ResolveSeriesReportCommand(seriesId, 7));

        // Assert
        await RunAsDefaultUserAsync();

        PaginatedList<NotificationVm>? notifications = await SendAsync(new GetUserNotificationsQuery());

        notifications.Items.First().Title.Should().Be("Vi phạm chính sách về xuất bản");
        notifications.Items.First().Content.Should()
            .Be(
                $"Do vi phạm chính sách về xuất bản. Bạn đã bị giới hạn tất cả các quyền sửa đổi truyện cho tới ngày {DateTimeOffset.UtcNow.AddDays(7):d}");
        notifications.Items.First().IsRead.Should().BeFalse();
    }
}
