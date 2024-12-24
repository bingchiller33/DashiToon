using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Reviews.Commands.RateReviewCommand;
using DashiToon.Api.Application.Reviews.Commands.WriteSeriesReviews;
using DashiToon.Api.Application.Reviews.Models;
using DashiToon.Api.Application.Reviews.Queries.GetSeriesReviews;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.Reviews.Queries;

using static Testing;

[Collection("Serialize")]
public class GetSeriesReviewsTest : BaseIntegrationTest
{
    public GetSeriesReviewsTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetSeriesReviewsShouldReturnSeriesReviews()
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

        ReviewVm? review1 = await SendAsync(new WriteSeriesReviewCommand(
            seriesId,
            """
            Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
            Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
            Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
            """,
            true));

        string? userId2 = await RunAsUserAsync("TestUser1", "Bruh@1311", []);
        ReviewVm? review2 = await SendAsync(new WriteSeriesReviewCommand(
            seriesId,
            """
            Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
            Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
            Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
            """,
            false));

        await SendAsync(new RateReviewCommand(
            seriesId,
            review1.Id,
            true
        ));

        await SendAsync(new RateReviewCommand(
            seriesId,
            review2.Id,
            true
        ));

        string? userId3 = await RunAsUserAsync("TestUser2", "Bruh@1311", []);
        ReviewVm? review3 = await SendAsync(new WriteSeriesReviewCommand(
            seriesId,
            """
            Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
            Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
            Nice Series Nice Series Nice Series Nice Series Nice Series Nice Series
            """,
            true));

        await SendAsync(new RateReviewCommand(
            seriesId,
            review3.Id,
            true
        ));

        await SendAsync(new RateReviewCommand(
            seriesId,
            review1.Id,
            false
        ));

        await SendAsync(new RateReviewCommand(
            seriesId,
            review2.Id,
            true
        ));

        List<Guid>? expectedOrder = new() { review2.Id, review3.Id, review1.Id };

        // Act  
        PaginatedList<ReviewVm>? result = await SendAsync(new GetSeriesReviewQuery(seriesId));

        // Assert
        result.TotalCount.Should().Be(3);
        result.Items.Select(i => i.Id).Should().BeEquivalentTo(expectedOrder);
    }
}
