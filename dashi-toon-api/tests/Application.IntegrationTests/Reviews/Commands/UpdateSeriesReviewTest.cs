using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Reviews.Commands.UpdateSeriesReviewCommand;
using DashiToon.Api.Application.Reviews.Commands.WriteSeriesReviews;
using DashiToon.Api.Application.Reviews.Models;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.Reviews.Commands;

using static Testing;

[Collection("Serialize")]
public class UpdateSeriesReviewTest : BaseIntegrationTest
{
    public UpdateSeriesReviewTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task UpdateSeriesReviewShouldUpdateReview()
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

        ReviewVm? review = await SendAsync(new WriteSeriesReviewCommand(
            seriesId,
            """
            Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
            Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
            Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
            """,
            true));

        // Act
        ReviewVm? updatedReview = await SendAsync(new UpdateReviewCommand(
            seriesId,
            review.Id,
            """
            Bad Series Bad Series Bad Series Bad Series Bad Series
            Bad Series Bad Series Bad Series Bad Series Bad Series
            Bad Series Bad Series Bad Series Bad Series Bad Series
            Bad Series Bad Series Bad Series Bad Series Bad Series
            """,
            false
        ));

        updatedReview.Id.Should().Be(review.Id);
        updatedReview.Content.Should().Be("""
                                          Bad Series Bad Series Bad Series Bad Series Bad Series
                                          Bad Series Bad Series Bad Series Bad Series Bad Series
                                          Bad Series Bad Series Bad Series Bad Series Bad Series
                                          Bad Series Bad Series Bad Series Bad Series Bad Series
                                          """);
        updatedReview.IsRecommended.Should().BeFalse();
        updatedReview.Likes.Should().Be(0);
        updatedReview.Dislikes.Should().Be(0);
        updatedReview.UserId.Should().Be(GetUserId());
        updatedReview.Username.Should().Be(GetUserName());
        updatedReview.IsEdited.Should().BeTrue();
    }


    [Fact]
    public async Task UpdateSeriesReviewShouldRequireExistingSeries()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        // Act
        await FluentActions.Invoking(() => SendAsync(new UpdateReviewCommand(
                1,
                Guid.Empty,
                """
                Bad Series Bad Series Bad Series Bad Series Bad Series
                Bad Series Bad Series Bad Series Bad Series Bad Series
                Bad Series Bad Series Bad Series Bad Series Bad Series
                Bad Series Bad Series Bad Series Bad Series Bad Series
                """,
                false
            )))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateSeriesReviewShouldRequireExistingReview()
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
        await FluentActions.Invoking(() => SendAsync(new UpdateReviewCommand(
                seriesId,
                Guid.Empty,
                """
                Bad Series Bad Series Bad Series Bad Series Bad Series
                Bad Series Bad Series Bad Series Bad Series Bad Series
                Bad Series Bad Series Bad Series Bad Series Bad Series
                Bad Series Bad Series Bad Series Bad Series Bad Series
                """,
                false
            )))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateSeriesReviewShouldOnlyAllowAuthor()
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

        ReviewVm? review = await SendAsync(new WriteSeriesReviewCommand(
            seriesId,
            """
            Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
            Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
            Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
            """,
            true));

        await RunAsUserAsync("AnotherGuy", "Bruh@1311", []);

        // Act
        await FluentActions.Invoking(() => SendAsync(new UpdateReviewCommand(
                seriesId,
                review.Id,
                """
                Bad Series Bad Series Bad Series Bad Series Bad Series
                Bad Series Bad Series Bad Series Bad Series Bad Series
                Bad Series Bad Series Bad Series Bad Series Bad Series
                Bad Series Bad Series Bad Series Bad Series Bad Series
                """,
                false
            )))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }
}
