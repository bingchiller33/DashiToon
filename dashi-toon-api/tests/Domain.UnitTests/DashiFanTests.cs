using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using FluentAssertions;

namespace Domain.UnitTests;

public class DashiFanTests
{
    [Fact]
    public void CreateDashiFanTierShouldCreateSuccessfully()
    {
        // Arrange
        string name = "TestTier";
        string description = "TestDescription";
        int perks = 4;
        int amount = 10_000;
        string currency = "USD";

        // Act
        DashiFan tier = DashiFan.Create(name, description, perks, amount, currency);

        // Assert
        tier.Should().NotBeNull();
        tier.Name.Should().Be(name);
        tier.Description.Should().Be(description);
        tier.Perks.Should().Be(perks);
        tier.Price.Amount.Should().Be(10_000);
        tier.Price.Currency.Should().Be("USD");
        tier.BillingCycle.Interval.Should().Be(BillingInterval.Monthly);
        tier.BillingCycle.IntervalCount.Should().Be(1);
        tier.IsActive.Should().Be(true);
    }

    [Theory]
    [MemberData(nameof(CreateDashiFanShouldRequireValidNameTestCases))]
    public void CreateDashiFanShouldRequireValidName(string name)
    {
        // Arrange
        string description = "TestDescription";
        int perks = 4;
        int amount = 10_000;
        string currency = "USD";

        FluentActions.Invoking(() => DashiFan.Create(name, description, perks, amount, currency))
            .Should().Throw<ArgumentException>();
    }

    public static IEnumerable<object[]> CreateDashiFanShouldRequireValidNameTestCases()
    {
        return
        [
            [string.Empty],
            [new string('*', 256)]
        ];
    }

    [Theory]
    [MemberData(nameof(CreateDashiFanShouldRequireValidDescriptionTestCases))]
    public void CreateDashiFanShouldRequireValidDescription(string description)
    {
        // Arrange
        string name = "TestTier";
        int perks = 4;
        int amount = 10_000;
        string currency = "USD";

        FluentActions.Invoking(() => DashiFan.Create(name, description, perks, amount, currency))
            .Should().Throw<ArgumentException>();
    }

    public static IEnumerable<object[]> CreateDashiFanShouldRequireValidDescriptionTestCases()
    {
        return
        [
            [string.Empty],
            [new string('*', 256)]
        ];
    }

    [Theory]
    [MemberData(nameof(CreateDashiFanShouldRequireValidPerksTestCases))]
    public void CreateDashiFanShouldRequireValidPerks(int perks)
    {
        // Arrange
        string name = "TestTier";
        string description = "TestDescription";
        int amount = 10_000;
        string currency = "USD";

        FluentActions.Invoking(() => DashiFan.Create(name, description, perks, amount, currency))
            .Should().Throw<ArgumentException>();
    }

    public static IEnumerable<object[]> CreateDashiFanShouldRequireValidPerksTestCases()
    {
        return
        [
            [0],
            [-1]
        ];
    }

    [Fact]
    public void UpdateDashiFanShouldUpdateSuccessfully()
    {
        // Arrange
        string name = "TestTier";
        string description = "TestDescription";
        int perks = 4;
        int amount = 10_000;
        string currency = "USD";

        DashiFan tier = DashiFan.Create(name, description, perks, amount, currency);
        // Act
        tier.Update("UpdatedTier", "UpdatedDescription", 10, 5_000);

        // Assert
        tier.Should().NotBeNull();
        tier.Name.Should().Be("UpdatedTier");
        tier.Description.Should().Be("UpdatedDescription");
        tier.Perks.Should().Be(10);
        tier.Price.Amount.Should().Be(5_000);
        tier.Price.Currency.Should().Be("USD");
        tier.BillingCycle.Interval.Should().Be(BillingInterval.Monthly);
        tier.BillingCycle.IntervalCount.Should().Be(1);
        tier.IsActive.Should().Be(true);
    }

    [Fact]
    public void UpdateDashiFanStatusShouldChangeStatus()
    {
        // Arrange
        string name = "TestTier";
        string description = "TestDescription";
        int perks = 4;
        int amount = 10_000;
        string currency = "USD";

        DashiFan tier = DashiFan.Create(name, description, perks, amount, currency);

        // Act
        tier.ChangeStatus();

        // Assert
        tier.Should().NotBeNull();
        tier.IsActive.Should().Be(false);
    }

    [Fact]
    public void UpdateDashiFanPlanIdShouldSuccess()
    {
        // Arrange
        string name = "TestTier";
        string description = "TestDescription";
        int perks = 4;
        int amount = 10_000;
        string currency = "USD";

        DashiFan tier = DashiFan.Create(name, description, perks, amount, currency);

        // Act
        tier.UpdatePlan("PlanId");

        // Assert
        tier.Should().NotBeNull();
        tier.PlanId.Should().Be("PlanId");
    }
}
