using DashiToon.Api.Application.Administrator.KanaGoldPacks.Commands.CreateKanaGoldPack;
using DashiToon.Api.Application.Administrator.KanaGoldPacks.Models;

namespace Application.IntegrationTests.Administrator.KanaGoldPacks.Commands;

using static Testing;

[Collection("Serialize")]
public class CreateKanaGoldPackTest : BaseIntegrationTest
{
    public CreateKanaGoldPackTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task CreateKanaGoldPackShouldSucceed()
    {
        // Arrange
        await RunAsAdministratorAsync();

        // Act
        KanaGoldPackVm? pack = await SendAsync(new CreateKanaGoldPackCommand(1000, 10_000, "VND"));

        // Assert
        pack.Should().NotBeNull();
        pack.Amount.Should().Be(1000);
        pack.Price.Amount.Should().Be(10_000);
        pack.Price.Currency.Should().Be("VND");
        pack.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateMissionShouldNotAllowedDuplicateAmountAndCurrencyPack()
    {
        // Arrange
        await RunAsAdministratorAsync();

        await SendAsync(new CreateKanaGoldPackCommand(1000, 10_000, "VND"));

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new CreateKanaGoldPackCommand(1000, 20_000, "VND")))
            .Should().ThrowAsync<ArgumentException>();
    }
}
