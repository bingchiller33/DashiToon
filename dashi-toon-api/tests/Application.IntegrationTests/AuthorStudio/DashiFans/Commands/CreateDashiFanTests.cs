using DashiToon.Api.Application.AuthorStudio.DashiFans.Commands.CreateDashiFan;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.AuthorStudio.DashiFans.Commands;

using static Testing;

[Collection("Serialize")]
public class CreateDashiFanTests : BaseIntegrationTest
{
    public CreateDashiFanTests(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task CreateDashiFanShouldSucceed()
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

        CreateDashiFanCommand command = new(
            seriesId,
            "TestTier",
            "TestDescription",
            4,
            100_000,
            "VND"
        );

        // Act
        Guid tierId = await SendAsync(command);

        // Assert
        DashiFan? tier = await FindAsync<DashiFan>(tierId);

        tier.Should().NotBeNull();
        tier!.Id.Should().Be(tierId);
        tier.SeriesId.Should().Be(seriesId);
        tier.Name.Should().Be("TestTier");
        tier.Description.Should().Be("TestDescription");
        tier.Perks.Should().Be(4);
        tier.IsActive.Should().Be(true);
        tier.Price.Amount.Should().Be(100_000);
        tier.Price.Currency.Should().Be("VND");
    }

    [Fact]
    public async Task CreateDashiFanShouldRequireExistingSeries()
    {
        await RunAsDefaultUserAsync();

        CreateDashiFanCommand command = new(
            1,
            "TestTier",
            "TestDescription",
            4,
            100_000,
            "VND"
        );
        await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateDashiFanShouldRequireAuthorizedUser()
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

        CreateDashiFanCommand command = new(
            seriesId,
            "TestTier",
            "TestDescription",
            4,
            100_000,
            "VND"
        );
        await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }
}
