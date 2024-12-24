using DashiToon.Api.Application.Users.Queries.GetKanaTotals;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.Users.Queries;

using static Testing;

[Collection("Serialize")]
public class GetKanaTotalsTest : BaseIntegrationTest
{
    public GetKanaTotalsTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetKanaTotalsShouldReturnKanaTotals()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        // Act
        KanaTotalsVm? kanaTotals = await SendAsync(new GetKanaTotalsQuery());

        // Assert
        kanaTotals.Totals[0].KanaType.Should().Be(KanaType.Coin);
        kanaTotals.Totals[0].Amount.Should().Be(0);
        kanaTotals.Totals[1].KanaType.Should().Be(KanaType.Gold);
        kanaTotals.Totals[1].Amount.Should().Be(0);
    }
}
