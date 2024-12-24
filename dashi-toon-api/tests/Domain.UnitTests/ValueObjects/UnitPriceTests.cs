using DashiToon.Api.Domain.ValueObjects;
using FluentAssertions;

namespace Domain.UnitTests.ValueObjects;

public class UnitPriceTests
{
    [Theory]
    [InlineData(50_000, "VND")]
    [InlineData(10_000, "VND")]
    [InlineData(5, "USD")]
    [InlineData(1, "USD")]
    public void CreateUnitPriceShouldCreateSuccessfully(decimal amount, string currency)
    {
        // Act
        Price price = Price.CreateNew(amount, currency);

        // Assert
        price.Should().NotBeNull();
        price.Amount.Should().Be(amount);
        price.Currency.Should().Be(currency);
    }

    [Theory]
    [InlineData(4.2, "PHP")]
    [InlineData(9_999, "VND")]
    [InlineData(100_000_001, "VND")]
    [InlineData(0.099, "USD")]
    [InlineData(10_001, "USD")]
    public void CreateUnitPriceShouldRequireValidAmountAndCurrency(decimal amount, string currency)
    {
        // Act
        FluentActions.Invoking(() => Price.CreateNew(amount, currency))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UnitPriceShouldBeEqual()
    {
        Price price1 = Price.CreateNew(10_000, "USD");
        Price price2 = Price.CreateNew(10_000, "USD");

        bool comparison = price1.Equals(price2);
        comparison.Should().BeTrue();
    }

    [Fact]
    public void UnitPriceShouldNotBeEqual()
    {
        Price price1 = Price.CreateNew(10_000, "USD");
        Price price2 = Price.CreateNew(5_000, "USD");

        bool comparison = price1.Equals(price2);
        comparison.Should().BeFalse();
    }
}
