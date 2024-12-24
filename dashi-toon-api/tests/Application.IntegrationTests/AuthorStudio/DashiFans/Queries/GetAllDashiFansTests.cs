using DashiToon.Api.Application.AuthorStudio.DashiFans.Commands.CreateDashiFan;
using DashiToon.Api.Application.AuthorStudio.DashiFans.Queries.GetDashiFans;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Domain.Enums;
using DashiToon.Api.Domain.ValueObjects;

namespace Application.IntegrationTests.AuthorStudio.DashiFans.Queries;

using static Testing;

[Collection("Serialize")]
public class GetAllDashiFansTests : BaseIntegrationTest
{
    public GetAllDashiFansTests(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetDashiFansShouldSucceed()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

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

        Guid tier1Id = await SendAsync(new CreateDashiFanCommand(
            seriesId,
            "TestTier1",
            "TestDescription1",
            4,
            100_000,
            "VND"
        ));

        Guid tier2Id = await SendAsync(new CreateDashiFanCommand(
            seriesId,
            "TestTier2",
            "TestDescription2",
            5,
            200_000,
            "VND"
        ));

        Guid tier3Id = await SendAsync(new CreateDashiFanCommand(
            seriesId,
            "TestTier3",
            "TestDescription3",
            6,
            300_000,
            "VND"
        ));

        List<DashiFanVm> expected = new()
        {
            new DashiFanVm(tier1Id, "TestTier1", Price.CreateNew(100_000, "VND"), "TestDescription1", 4, true, ""),
            new DashiFanVm(tier2Id, "TestTier2", Price.CreateNew(200_000, "VND"), "TestDescription2", 5, true, ""),
            new DashiFanVm(tier3Id, "TestTier3", Price.CreateNew(300_000, "VND"), "TestDescription3", 6, true, "")
        };

        // Act
        List<DashiFanVm> tiers = await SendAsync(new GetDashiFansQuery(seriesId));

        // Assert
        tiers.Should().BeEquivalentTo(expected, options => options.Excluding(info => info.LastModified));
    }


    [Fact]
    public async Task GetDashiFansShouldRequireExistingSeries()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        GetDashiFansQuery query = new(1);

        // Act
        await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetDashiFansShouldRequireAuthorizedUser()
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

        await RunAsUserAsync("TestUser", "NewUser@123", []);

        GetDashiFansQuery query = new(seriesId);

        // Act
        await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }
}
