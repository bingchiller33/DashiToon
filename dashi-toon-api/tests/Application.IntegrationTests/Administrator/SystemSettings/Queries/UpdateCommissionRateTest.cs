using DashiToon.Api.Application.Administrator.SystemSettings.Commands.UpdateCommissionRate;
using DashiToon.Api.Application.Administrator.SystemSettings.Models;
using DashiToon.Api.Application.Administrator.SystemSettings.Queries.GetCommissionRate;
using DashiToon.Api.Domain.Entities;

namespace Application.IntegrationTests.Administrator.SystemSettings.Queries;

using static Testing;

[Collection("Serialize")]
public class UpdateCommissionRateTest : BaseIntegrationTest
{
    public UpdateCommissionRateTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task UpdateCommissionRateShouldUpdateRate()
    {
        // Arrange
        await RunAsAdministratorAsync();

        // Act
        await SendAsync(new UpdateCommissionRateCommand(CommissionType.Kana, 40));

        // Assert
        CommissionRateVm? commissionRate = await SendAsync(new GetCommissionRateQuery(CommissionType.Kana));

        commissionRate.Rate.Should().Be(40);
    }
}
