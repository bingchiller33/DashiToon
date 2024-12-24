using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using DashiToon.Api.Domain.Exceptions;
using DashiToon.Api.Domain.Services;
using DashiToon.Api.Domain.ValueObjects;
using DashiToon.Api.Infrastructure.Identity;
using FluentAssertions;

namespace Domain.UnitTests;

public class KanaServiceTests
{
    [Fact]
    public void CreatePurchaseOrderShouldSuccess()
    {
        // Arrange
        string? orderId = Guid.NewGuid().ToString();
        ApplicationUser? user = new();
        KanaGoldPack? pack = KanaGoldPack.Create(
            69,
            Price.CreateNew(10, "USD"),
            true);

        // Act
        PurchaseOrder? order = KanaService.CreatePurchaseOrder(orderId, user, pack);

        // Assert
        order.Should().NotBeNull();
        order.UserId.Should().Be(user.Id);
        order.PackId.Should().Be(pack.Id);
        order.Price.Should().Be(pack.Price);
        order.Status.Should().Be(OrderStatus.Pending);
        order.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void CreatePurchaseOrderShouldRequireActivePack()
    {
        // Arrange
        string? orderId = Guid.NewGuid().ToString();
        ApplicationUser? user = new();
        KanaGoldPack? pack = KanaGoldPack.Create(
            69,
            Price.CreateNew(10, "USD"),
            false);

        // Act
        FluentActions.Invoking(() => KanaService.CreatePurchaseOrder(orderId, user, pack))
            .Should().Throw<KanaGoldPackInactiveException>();
    }

    [Fact]
    public void CancelOrderShouldChangeOrderStatus()
    {
        // Arrange
        string? orderId = Guid.NewGuid().ToString();
        ApplicationUser? user = new();
        KanaGoldPack? pack = KanaGoldPack.Create(
            69,
            Price.CreateNew(10, "USD"),
            true);

        PurchaseOrder? order = KanaService.CreatePurchaseOrder(orderId, user, pack);

        // Act
        order.CancelOrder();

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CompletedAt.Should().BeNull();
    }
}
