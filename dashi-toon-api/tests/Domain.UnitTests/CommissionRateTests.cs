using DashiToon.Api.Domain.Entities;
using FluentAssertions;

namespace Domain.UnitTests;

public class CommissionRateTests
{
    [Fact]
    public void CreateCommissionRateShouldSuccess()
    {
        // Arrange
        CommissionType type = CommissionType.Kana;
        decimal rate = 50M;

        // Act
        CommissionRate? commissionRate = CommissionRate.Create(type, rate);

        // Assert
        commissionRate.Type.Should().Be(type);
        commissionRate.RatePercentage.Should().Be(rate);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void CreateCommissionRateShouldRequireValidRate(decimal rate)
    {
        // Arrange
        CommissionType type = CommissionType.Kana;

        FluentActions.Invoking(() => CommissionRate.Create(type, rate))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateRateCommissionRateShouldRate()
    {
        // Arrange
        CommissionRate? commissionRate = CommissionRate.Create(CommissionType.DashiFan, 20M);

        // Act
        commissionRate.UpdateRate(30);

        // Assert
        commissionRate.RatePercentage.Should().Be(30);
    }
}
