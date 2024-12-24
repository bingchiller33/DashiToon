using DashiToon.Api.Application.AuthorStudio.DashiFans.Commands.CreateDashiFan;
using DashiToon.Api.Application.AuthorStudio.DashiFans.Commands.UpdateDashiFanStatus;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.SeriesDetail.Queries.GetSeriesDashiFans;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.SeriesDetail.Queries;

using static Testing;

[Collection("Serialize")]
public class GetSeriesDashiFansTest : BaseIntegrationTest
{
    public GetSeriesDashiFansTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetSeriesDashiFansShouldReturnActiveTiers()
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
            null
        ));

        Guid tierId1 = await SendAsync(new CreateDashiFanCommand(
            seriesId,
            "TestTier1",
            "TestDescription1",
            4,
            100_000,
            "VND"
        ));

        Guid tierId2 = await SendAsync(new CreateDashiFanCommand(
            seriesId,
            "TestTier2",
            "TestDescription2",
            5,
            200_000,
            "VND"
        ));

        Guid tierId3 = await SendAsync(new CreateDashiFanCommand(seriesId,
            "TestTier3",
            "TestDescription3",
            6,
            300_000,
            "VND"));

        await SendAsync(new UpdateDashiFanStatusCommand(seriesId, tierId3));

        // Act
        List<SeriesDashiFanVm>? result = await SendAsync(new GetSeriesDashiFansQuery(seriesId));

        // Assert
        result.Should().HaveCount(2);
        SeriesDashiFanVm? tier1 = result[0];
        SeriesDashiFanVm? tier2 = result[1];
        tier1.Should().NotBeNull();
        tier1.Id.Should().Be(tierId1);
        tier1.Name.Should().Be("TestTier1");
        tier1.Description.Should().Be("TestDescription1");
        tier1.Perks.Should().Be(4);
        tier1.Price.Amount.Should().Be(100_000);
        tier1.Price.Currency.Should().Be("VND");

        tier2.Should().NotBeNull();
        tier2.Id.Should().Be(tierId2);
        tier2.Name.Should().Be("TestTier2");
        tier2.Description.Should().Be("TestDescription2");
        tier2.Perks.Should().Be(5);
        tier2.Price.Amount.Should().Be(200_000);
        tier2.Price.Currency.Should().Be("VND");
    }

    [Fact]
    public async Task GetSeriesDashiFansShouldReturnEmptyForNonActiveSeries()
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

        Guid tierId1 = await SendAsync(new CreateDashiFanCommand(
            seriesId,
            "TestTier1",
            "TestDescription1",
            4,
            100_000,
            "VND"
        ));

        Guid tierId2 = await SendAsync(new CreateDashiFanCommand(
            seriesId,
            "TestTier2",
            "TestDescription2",
            5,
            200_000,
            "VND"
        ));

        // Act
        List<SeriesDashiFanVm>? result = await SendAsync(new GetSeriesDashiFansQuery(seriesId));

        // Assert
        result.Should().BeEmpty();
    }
}
