using DashiToon.Api.Domain.Enums;
using DashiToon.Api.Domain.ValueObjects;
using FluentAssertions;

namespace Domain.UnitTests.ValueObjects;

public class BillingCycleTests
{
    [Theory]
    [InlineData(BillingInterval.Daily, 30)]
    [InlineData(BillingInterval.Weekly, 4)]
    [InlineData(BillingInterval.Monthly, 1)]
    [InlineData(BillingInterval.Yearly, 2)]
    public void CreateBillingCycleShouldCreateSuccessfully(BillingInterval interval, int intervalCount)
    {
        // Act
        BillingCycle cycle = BillingCycle.CreateNew(interval, intervalCount);

        // Assert
        cycle.Should().NotBeNull();
        cycle.Interval.Should().Be(interval);
        cycle.IntervalCount.Should().Be(intervalCount);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void CreateBillingCycleShouldRequireValidIntervalCount(int intervalCount)
    {
        // Act
        FluentActions.Invoking(() => BillingCycle.CreateNew(BillingInterval.Daily, intervalCount))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void BillingCycleShouldBeEqual()
    {
        BillingCycle billingCycle1 = BillingCycle.CreateNew(BillingInterval.Weekly, 2);
        BillingCycle billingCycle2 = BillingCycle.CreateNew(BillingInterval.Weekly, 2);

        bool comparison = billingCycle1.Equals(billingCycle2);
        comparison.Should().BeTrue();
    }

    [Fact]
    public void BillingCycleShouldNotBeEqual()
    {
        BillingCycle billingCycle1 = BillingCycle.CreateNew(BillingInterval.Weekly, 2);
        BillingCycle billingCycle2 = BillingCycle.CreateNew(BillingInterval.Weekly, 3);

        bool comparison = billingCycle1.Equals(billingCycle2);
        comparison.Should().BeFalse();
    }
}
