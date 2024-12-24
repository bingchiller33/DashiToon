using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.Reviews.Commands.WriteSeriesReviews;
using DashiToon.Api.Application.Reviews.Models;
using DashiToon.Api.Domain.Enums;
using DashiToon.Api.Domain.Exceptions;

namespace Application.IntegrationTests.Reviews.Commands;

using static Testing;

[Collection("Serialize")]
public class WriteSeriesReviewTest : BaseIntegrationTest
{
    public WriteSeriesReviewTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task WriteSeriesReviewShouldPersistNewReview()
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
        ReviewVm? review = await SendAsync(new WriteSeriesReviewCommand(
            seriesId,
            """
            Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
            Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
            Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
            """,
            true));

        // Assert
        review.Should().NotBeNull();
        review.Content.Should().Be("""
                                   Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
                                   Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
                                   Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
                                   """);
        review.IsRecommended.Should().BeTrue();
        review.Likes.Should().Be(0);
        review.Dislikes.Should().Be(0);
        review.UserId.Should().Be(GetUserId());
        review.Username.Should().Be(GetUserName());
        review.IsEdited.Should().BeFalse();
    }

    [Fact]
    public async Task WriteSeriesReviewShouldRequireExistingSeries()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        // Act && Assert
        await FluentActions.Invoking(() => SendAsync(new WriteSeriesReviewCommand(
                1,
                """
                Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
                Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
                Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
                """,
                true)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task WriteSeriesReviewShouldNotAllowUserToReviewTwice()
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

        await SendAsync(new WriteSeriesReviewCommand(
            seriesId,
            """
            Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
            Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
            Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
            """,
            true));

        // Act && Assert
        await FluentActions.Invoking(() => SendAsync(new WriteSeriesReviewCommand(
                seriesId,
                """
                Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
                Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
                Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
                """,
                true)))
            .Should().ThrowAsync<AlreadyReviewedException>();
    }
}
