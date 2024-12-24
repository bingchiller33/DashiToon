using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.ValueObjects;
using FluentAssertions;

namespace Domain.UnitTests;

public class KanaGoldPackTest
{
    [Fact]
    public void CreateKanaGoldPackShouldCreateSuccessfully()
    {
        // Arrange
        int amount = 100;

        Price price = Price.CreateNew(6.9M, "USD");

        bool isAcitve = true;
        // Act
        KanaGoldPack pack = KanaGoldPack.Create(amount, price, isAcitve);

        // Assert
        pack.Should().NotBeNull();
        pack.Amount.Should().Be(amount);
        pack.Price.Should().Be(price);
        pack.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateKanaGoldPackShouldValidateAmount(int amount)
    {
        // Arrange
        Price price = Price.CreateNew(10_000M, "VND");

        FluentActions.Invoking(() => KanaGoldPack.Create(amount, price, true))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateKanaGoldPackShouldUpdateSuccessfully()
    {
        // Arrange
        KanaGoldPack? pack = KanaGoldPack.Create(100, Price.CreateNew(10_000M, "VND"), false);

        int newAmount = 200;
        Price? newPrice = Price.CreateNew(20_000, "VND");
        bool isAcitve = true;

        // Act
        pack.Update(newAmount, newPrice, isAcitve);

        // Assert
        pack.Amount.Should().Be(newAmount);
        pack.Price.Should().Be(newPrice);
        pack.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateKanaGoldPackStatusShouldUpdateStatus()
    {
        // Arrange
        KanaGoldPack? pack = KanaGoldPack.Create(100, Price.CreateNew(10_000M, "VND"), false);

        // Act
        pack.UpdateStatus(true);

        // Assert
        pack.IsActive.Should().BeTrue();
    }
}
