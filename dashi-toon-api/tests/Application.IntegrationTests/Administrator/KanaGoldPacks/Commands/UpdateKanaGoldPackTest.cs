using DashiToon.Api.Application.Administrator.KanaGoldPacks.Commands.CreateKanaGoldPack;
using DashiToon.Api.Application.Administrator.KanaGoldPacks.Commands.UpdateKanaGoldPack;
using DashiToon.Api.Application.Administrator.KanaGoldPacks.Models;

namespace Application.IntegrationTests.Administrator.KanaGoldPacks.Commands;

using static Testing;

[Collection("Serialize")]
public class UpdateKanaGoldPackTest : BaseIntegrationTest
{
    public UpdateKanaGoldPackTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task UpdateKanaGoldPackShouldSucceed()
    {
        // Arrange
        await RunAsAdministratorAsync();

        KanaGoldPackVm? pack = await SendAsync(new CreateKanaGoldPackCommand(1000, 10_000, "VND"));

        // Act
        KanaGoldPackVm? updatedPack =
            await SendAsync(new UpdateKanaGoldPackCommand(pack.Id, 2000, 20_000, "VND", false));

        // Assert
        updatedPack.Should().NotBeNull();
        updatedPack.Amount.Should().Be(2000);
        updatedPack.Price.Amount.Should().Be(20_000);
        updatedPack.Price.Currency.Should().Be("VND");
        updatedPack.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdatePackShouldNotAllowedDuplicateAmountAndCurrencyPack()
    {
        // Arrange
        await RunAsAdministratorAsync();

        KanaGoldPackVm? pack1 = await SendAsync(new CreateKanaGoldPackCommand(1000, 10_000, "VND"));

        KanaGoldPackVm? pack2 = await SendAsync(new CreateKanaGoldPackCommand(2000, 10_000, "USD"));

        // Act & Assert
        await FluentActions
            .Invoking(() => SendAsync(new UpdateKanaGoldPackCommand(pack2.Id, 1000, 10_000, "VND", true)))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdatePackShouldRequireExistingPack()
    {
        // Arrange
        await RunAsAdministratorAsync();

        // Act & Assert
        await FluentActions.Invoking(() =>
                SendAsync(new UpdateKanaGoldPackCommand(Guid.NewGuid(), 1000, 10_000, "VND", true)))
            .Should().ThrowAsync<NotFoundException>();
    }
}
