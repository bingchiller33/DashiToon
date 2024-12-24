using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using DashiToon.Api.Domain.ValueObjects;
using DashiToon.Api.Infrastructure.Identity;
using FluentAssertions;

namespace Domain.UnitTests;

public class PurchaseOrderTests
{
    [Fact]
    public void CreatePurchaseOrderShouldSuccess()
    {
        // Arrange
        string? orderId = Guid.NewGuid().ToString();
        ApplicationUser? user = new();
        KanaGoldPack? pack = KanaGoldPack.Create(69, Price.CreateNew(10, "USD"), true);

        // Act
        PurchaseOrder? order = PurchaseOrder.CreateNew(orderId, user.Id, pack.Id, pack.Price);

        // Assert
        order.Should().NotBeNull();
        order.Id.Should().Be(orderId);
        order.UserId.Should().Be(user.Id);
        order.PackId.Should().Be(pack.Id);
        order.Price.Should().Be(pack.Price);
        order.Status.Should().Be(OrderStatus.Pending);
        order.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void CompletePurchaseOrderShouldSuccess()
    {
        // Arrange
        string? orderId = Guid.NewGuid().ToString();
        ApplicationUser? user = new();
        KanaGoldPack? pack = KanaGoldPack.Create(69, Price.CreateNew(10, "USD"), true);

        PurchaseOrder? order = PurchaseOrder.CreateNew(orderId, user.Id, pack.Id, pack.Price);

        // Act
        order.CompleteOrder();

        // Assert
        order.Should().NotBeNull();
        order.Id.Should().Be(orderId);
        order.UserId.Should().Be(user.Id);
        order.PackId.Should().Be(pack.Id);
        order.Price.Should().Be(pack.Price);
        order.Status.Should().Be(OrderStatus.Success);
        order.CompletedAt.Should().BeWithin(TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public void CancelPurchaseOrderShouldSuccess()
    {
        // Arrange
        string? orderId = Guid.NewGuid().ToString();
        ApplicationUser? user = new();
        KanaGoldPack? pack = KanaGoldPack.Create(69, Price.CreateNew(10, "USD"), true);

        PurchaseOrder? order = PurchaseOrder.CreateNew(orderId, user.Id, pack.Id, pack.Price);
        // Act
        order.CancelOrder();

        // Assert
        order.Should().NotBeNull();
        order.Id.Should().Be(orderId);
        order.UserId.Should().Be(user.Id);
        order.PackId.Should().Be(pack.Id);
        order.Price.Should().Be(pack.Price);
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CompletedAt.Should().BeNull();
    }
}
