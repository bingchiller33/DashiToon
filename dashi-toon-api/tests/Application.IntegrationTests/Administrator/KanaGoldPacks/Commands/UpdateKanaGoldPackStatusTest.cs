using DashiToon.Api.Application.Administrator.KanaGoldPacks.Commands.CreateKanaGoldPack;
using DashiToon.Api.Application.Administrator.KanaGoldPacks.Commands.UpdateKanaGoldPackStatus;
using DashiToon.Api.Application.Administrator.KanaGoldPacks.Models;

namespace Application.IntegrationTests.Administrator.KanaGoldPacks.Commands;

using static Testing;

[Collection("Serialize")]
public class UpdateKanaGoldPackStatusTest : BaseIntegrationTest
{
    public UpdateKanaGoldPackStatusTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task UpdatePackStatusShouldChangePackStatus()
    {
        // Arrange
        await RunAsAdministratorAsync();

        KanaGoldPackVm? pack = await SendAsync(new CreateKanaGoldPackCommand(1000, 10_000, "VND"));

        // Act
        KanaGoldPackVm? updatedPack = await SendAsync(new UpdateKanaGoldPackStatusCommand(pack.Id, false));

        // Assert
        updatedPack.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdatePackStatusShouldRequireExistingPack()
    {
        // Arrange
        await RunAsAdministratorAsync();

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new UpdateKanaGoldPackStatusCommand(Guid.NewGuid(), false)))
            .Should().ThrowAsync<NotFoundException>();
    }
}
