using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.Reviews.Commands.WriteSeriesReviews;
using DashiToon.Api.Application.Reviews.Models;
using DashiToon.Api.Application.Reviews.Queries.GetCurrentUserReview;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.Reviews.Queries;

using static Testing;

[Collection("Serialize")]
public class GetCurrentUserReviewTest : BaseIntegrationTest
{
    public GetCurrentUserReviewTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetCurrentUserReviewShouldReturnReview()
    {
        // Arrange
        string? userId = await RunAsDefaultUserAsync();

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

        ReviewVm? review = await SendAsync(new WriteSeriesReviewCommand(
            seriesId,
            """
            Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
            Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
            Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
            """,
            true));

        // Act
        ReviewVm? currentUserReview = await SendAsync(new GetCurrentUserReviewQuery(seriesId));

        // Assert
        currentUserReview.Should().NotBeNull();
        currentUserReview!.Id.Should().Be(review.Id);
        currentUserReview.Content.Should().Be("""
                                              Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
                                              Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
                                              Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
                                              """);
        currentUserReview.IsRecommended.Should().BeTrue();
        currentUserReview.Likes.Should().Be(0);
        currentUserReview.Dislikes.Should().Be(0);
        currentUserReview.UserId.Should().Be(userId);
        currentUserReview.Username.Should().Be(GetUserName());
        currentUserReview.IsEdited.Should().BeFalse();
    }

    [Fact]
    public async Task GetCurrentUserReviewShouldReturnNullIfUserHaveNoReview()
    {
        // Arrange
        string? userId = await RunAsDefaultUserAsync();

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
        ReviewVm? currentUserReview = await SendAsync(new GetCurrentUserReviewQuery(seriesId));

        // Assert
        currentUserReview.Should().BeNull();
    }
}
