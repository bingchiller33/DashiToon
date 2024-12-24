using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using FluentAssertions;

namespace Domain.UnitTests;

public class KanaTransactionTests
{
    [Fact]
    public void CreateKanaTransactionShouldCreateSuccessfully()
    {
        // Arrange
        KanaType currency = KanaType.Coin;
        TransactionType type = TransactionType.Checkin;
        int amount = 694200;
        string reason = "Checkin";

        // Act
        KanaTransaction transaction = KanaTransaction.Create(currency, type, amount, reason);

        // Assert
        transaction.Should().NotBeNull();
        transaction.Currency.Should().Be(currency);
        transaction.Type.Should().Be(type);
        transaction.Amount.Should().Be(amount);
        transaction.Reason.Should().Be(reason);
        transaction.Timestamp.Should().BeWithin(TimeSpan.FromMilliseconds(100));
    }
}
