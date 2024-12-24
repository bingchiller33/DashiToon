using DashiToon.Api.Application.Administrator.SystemSettings.Models;
using DashiToon.Api.Application.Administrator.SystemSettings.Queries.GetCommissionRate;
using DashiToon.Api.Domain.Entities;

namespace Application.IntegrationTests.Administrator.SystemSettings.Commands;

using static Testing;

[Collection("Serialize")]
public class GetCommissionRateTest : BaseIntegrationTest
{
    public GetCommissionRateTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetCommissionRateShouldReturnCommissionRate()
    {
        // Arrange
        await RunAsAdministratorAsync();

        // Act
        CommissionRateVm? kanaRate = await SendAsync(new GetCommissionRateQuery(CommissionType.Kana));
        CommissionRateVm? dashiFanRate = await SendAsync(new GetCommissionRateQuery(CommissionType.DashiFan));

        // Assert
        kanaRate.Should().NotBeNull();
        kanaRate.Rate.Should().Be(30M);
        dashiFanRate.Should().NotBeNull();
        dashiFanRate.Rate.Should().Be(30M);
    }
}
