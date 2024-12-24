using DashiToon.Api.Domain.Entities;
using FluentAssertions;

namespace Domain.UnitTests;

public class KanaExchangeRateTests
{
    [Fact]
    public void CreateKanaExchangeRateShouldCreateSuccessfully()
    {
        // Arrange
        string? targetCurrency = "VND";
        int rate = 100;

        // Act
        KanaExchangeRate? kanaRate = KanaExchangeRate.Create(targetCurrency, rate);

        // Assert
        kanaRate.Rate.Should().Be(rate);
        kanaRate.TargetCurrency.Should().Be(targetCurrency);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateKanaExchangeRateShouldRequireValidRate(decimal rate)
    {
        // Arrange
        string? targetCurrency = "VND";

        // Act
        FluentActions.Invoking(() => KanaExchangeRate.Create(targetCurrency, rate))
            .Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("PHP")]
    public void CreateKanaExchangeRateShouldRequireValidCurrency(string currency)
    {
        // Arrange
        int rate = 10;

        // Act
        FluentActions.Invoking(() => KanaExchangeRate.Create(currency, rate))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateRateShouldChangeRate()
    {
        // Arrange
        string? targetCurrency = "VND";
        int rate = 100;

        KanaExchangeRate? kanaRate = KanaExchangeRate.Create(targetCurrency, rate);

        // Act
        kanaRate.UpdateRate(50);

        // Assert
        kanaRate.Rate.Should().Be(50);
        kanaRate.TargetCurrency.Should().Be(targetCurrency);
    }
}
