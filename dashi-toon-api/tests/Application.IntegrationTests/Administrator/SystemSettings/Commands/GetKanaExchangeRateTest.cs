using DashiToon.Api.Application.Administrator.SystemSettings.Models;
using DashiToon.Api.Application.Administrator.SystemSettings.Queries.GetKanaExchangeRate;
using DashiToon.Api.Domain.Entities;

namespace Application.IntegrationTests.Administrator.SystemSettings.Commands;

using static Testing;

[Collection("Serialize")]
public class GetKanaExchangeRateTest : BaseIntegrationTest
{
    public GetKanaExchangeRateTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetKanaExchangeRateShouldReturnExchangeRate()
    {
        // Arrange
        await RunAsAdministratorAsync();

        // Act
        KanaExchangeRateVm? kanaExchangeRate = await SendAsync(new GetKanaExchangeRateQuery());

        // Assert
        kanaExchangeRate.Should().NotBeNull();
        kanaExchangeRate.Rate.Should().Be(10);
        kanaExchangeRate.Currency.Should().Be("VND");
    }
}
