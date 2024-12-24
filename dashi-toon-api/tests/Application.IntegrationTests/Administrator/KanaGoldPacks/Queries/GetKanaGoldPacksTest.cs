using DashiToon.Api.Application.Administrator.KanaGoldPacks.Commands.CreateKanaGoldPack;
using DashiToon.Api.Application.Administrator.KanaGoldPacks.Queries.GetKanaGoldPacks;
using KanaGoldPackVm = DashiToon.Api.Application.Administrator.KanaGoldPacks.Models.KanaGoldPackVm;

namespace Application.IntegrationTests.Administrator.KanaGoldPacks.Queries;

using static Testing;

[Collection("Serialize")]
public class GetKanaGoldPacksTest : BaseIntegrationTest
{
    public GetKanaGoldPacksTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetKanaGoldPacksShouldReturnKanaGoldPacks()
    {
        // Arrange
        await RunAsAdministratorAsync();

        KanaGoldPackVm? pack1 = await SendAsync(new CreateKanaGoldPackCommand(1000, 10_000, "VND"));

        KanaGoldPackVm? pack2 = await SendAsync(new CreateKanaGoldPackCommand(2000, 10_000, "USD"));

        List<KanaGoldPackVm>? expected = new() { pack1, pack2 };

        // Act
        List<KanaGoldPackVm>? packs = await SendAsync(new GetKanaGoldPackQuery());

        // Assert
        packs.Should().BeEquivalentTo(expected);
    }
}
