using System.Data;
using Dapper;
using DashiToon.Api.Application.AuthorStudio.DashiFans.Commands.CreateDashiFan;
using DashiToon.Api.Application.AuthorStudio.DashiFans.Commands.UpdateDashiFan;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using DashiToon.Api.Domain.Exceptions;

namespace Application.IntegrationTests.AuthorStudio.DashiFans.Commands;

using static Testing;

[Collection("Serialize")]
public class UpdateDashiFanTest : BaseIntegrationTest
{
    public UpdateDashiFanTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task UpdateDashiFanShouldSucceed()
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

        Guid tierId = await SendAsync(new CreateDashiFanCommand(
            seriesId,
            "TestTier",
            "TestDescription",
            4,
            100_000,
            "VND"
        ));

        IDbConnection? connection = GetConnection();
        await connection.ExecuteAsync(
            """
            UPDATE "DashiFans"
            SET "Created" = @Created, "LastModified" = @LastModified
            WHERE "Id" = @Id
            """,
            new
            {
                Id = tierId,
                Created = DateTimeOffset.UtcNow.AddDays(-30),
                LastModified = DateTimeOffset.UtcNow.AddDays(-30)
            }
        );

        UpdateDashiFanCommand? updateDashiFanCommand = new(
            seriesId,
            tierId,
            "Updated Tier",
            "Updated Description",
            5,
            200_000
        );

        // Act
        await SendAsync(updateDashiFanCommand);

        // Assert
        DashiFan? tier = await FindAsync<DashiFan>(tierId);

        tier.Should().NotBeNull();
        tier!.Name.Should().Be("Updated Tier");
        tier.Description.Should().Be("Updated Description");
        tier.Perks.Should().Be(5);
        tier.Price.Amount.Should().Be(200_000);
    }

    [Fact]
    public async Task UpdateDashiFanShouldNotAllowedModificationWithin30Days()
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

        Guid tierId = await SendAsync(new CreateDashiFanCommand(
            seriesId,
            "TestTier",
            "TestDescription",
            4,
            100_000,
            "VND"
        ));

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new UpdateDashiFanCommand(
                seriesId,
                tierId,
                "Updated Tier",
                "Updated Description",
                5,
                200_000
            )))
            .Should().ThrowAsync<UpdateDashiFanCoolDownException>();
    }

    [Fact]
    public async Task UpdateDashiFanShouldRequireExistingSeries()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

        UpdateDashiFanCommand? updateDashiFanCommand = new(
            1,
            Guid.Empty,
            "Updated Tier",
            "Updated Description",
            5,
            200_000
        );

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(updateDashiFanCommand))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateDashiFanShouldOnlyAllowedAuthor()
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


        UpdateDashiFanCommand? updateDashiFanCommand = new(
            seriesId,
            Guid.Empty,
            "Updated Tier",
            "Updated Description",
            5,
            200_000
        );

        await RunAsUserAsync("TestUser", "NewUser@123", []);

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(updateDashiFanCommand))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task UpdateDashiFanShoulRequireExistingTier()
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

        UpdateDashiFanCommand? updateDashiFanCommand = new(
            seriesId,
            Guid.Empty,
            "Updated Tier",
            "Updated Description",
            5,
            200_000
        );

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(updateDashiFanCommand))
            .Should().ThrowAsync<DashiFanTierNotFoundException>();
    }
}
