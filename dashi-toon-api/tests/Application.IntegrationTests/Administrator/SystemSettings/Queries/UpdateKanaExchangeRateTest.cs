using DashiToon.Api.Application.Administrator.SystemSettings.Commands.UpdateKanaExchangeRate;
using DashiToon.Api.Application.Administrator.SystemSettings.Models;
using DashiToon.Api.Application.Administrator.SystemSettings.Queries.GetKanaExchangeRate;

namespace Application.IntegrationTests.Administrator.SystemSettings.Queries;

using static Testing;

[Collection("Serialize")]
public class UpdateKanaExchangeRateTest : BaseIntegrationTest
{
    public UpdateKanaExchangeRateTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task UpdateKanaExchangeRateShouldUpdateRate()
    {
        // Arrange
        await RunAsAdministratorAsync();

        // Act
        await SendAsync(new UpdateKanaExchangeRateCommand(20));

        // Assert
        KanaExchangeRateVm? exchangeRate = await SendAsync(new GetKanaExchangeRateQuery());

        exchangeRate.Currency.Should().Be("VND");
        exchangeRate.Rate.Should().Be(20);
    }
}
