using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Queries.GetAllSeries;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.AuthorStudio.Series.Queries;

using static Testing;

[Collection("Serialize")]
public class GetAllSeriesTest : BaseIntegrationTest
{
    public GetAllSeriesTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetAllSeriesShouldReturnAllSeries()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        await SendAsync(new CreateSeriesCommand(
            "TestTitle1",
            "TestSynopsis1",
            "TestThumbnail11",
            SeriesType.Comic,
            [1, 2, 3],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 2),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 2),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
            ],
            ["Alt 1", "Alt 2"],
            ["Author 1"],
            null));

        await SendAsync(new CreateSeriesCommand(
            "TestTitle2",
            "TestSynopsis2",
            "TestThumbnail2",
            SeriesType.Novel,
            [4, 5, 6],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 3),
                new ContentQuestionnaire(ContentCategory.Nudity, 3),
                new ContentQuestionnaire(ContentCategory.Profanity, 3),
                new ContentQuestionnaire(ContentCategory.Alcohol, 3),
                new ContentQuestionnaire(ContentCategory.Sensitive, 3),
                new ContentQuestionnaire(ContentCategory.Sexual, 3)
            ],
            ["Alt 3", "Alt 4"],
            ["Author 2"],
            null));

        // Act
        List<SeriesVm> seriesVms = await SendAsync(new GetAllSeriesQuery());

        // Assert
        seriesVms.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllSeriesShouldReturnEmptyWhenUserHasNoSeries()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

        // Act
        List<SeriesVm> seriesVms = await SendAsync(new GetAllSeriesQuery());

        // Assert
        seriesVms.Should().HaveCount(0);
    }
}
