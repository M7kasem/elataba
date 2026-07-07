using ElAtaba.Domain.Entities;
using ElAtaba.Domain.Enums;
using Elattba.Application.Orders;
using Elattba.Core.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Elattba.Tests;

public sealed class OrderServiceTests
{
    [Fact]
    public async Task CreateAsync_rejects_order_without_items()
    {
        var unitOfWork = SeedOrderDependencies();
        var service = new OrderService(unitOfWork);

        var result = await service.CreateAsync(NewOrder([]));

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_rejects_invalid_quantity()
    {
        var unitOfWork = SeedOrderDependencies();
        var service = new OrderService(unitOfWork);

        var result = await service.CreateAsync(NewOrder([new CreateOrderItemDto(1, 0)]));

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_rejects_products_from_different_store()
    {
        var unitOfWork = SeedOrderDependencies();
        unitOfWork.ProductsRepo.Items.Add(new Product { ProductId = 2, StoreId = 99, BasePrice = 20, StockQuantity = 10 });
        var service = new OrderService(unitOfWork);

        var result = await service.CreateAsync(NewOrder([new CreateOrderItemDto(2, 1)]));

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_rejects_insufficient_stock()
    {
        var unitOfWork = SeedOrderDependencies();
        unitOfWork.ProductsRepo.Items[0].StockQuantity = 1;
        var service = new OrderService(unitOfWork);

        var result = await service.CreateAsync(NewOrder([new CreateOrderItemDto(1, 2)]));

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_uses_active_offer_price()
    {
        var unitOfWork = SeedOrderDependencies();
        unitOfWork.OffersRepo.Items.Add(new Offer
        {
            OfferId = 1,
            StoreId = 1,
            DiscountPercentage = 25,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            AppliesToAllProducts = true
        });
        var service = new OrderService(unitOfWork);

        var result = await service.CreateAsync(NewOrder([new CreateOrderItemDto(1, 2)]));

        Assert.True(result.Succeeded);
        var order = Assert.Single(unitOfWork.OrdersRepo.Items);
        var item = Assert.Single(order.OrderItems);
        Assert.Equal(75, item.UnitPrice);
        Assert.Equal(150, item.Subtotal);
        Assert.Equal(150, order.TotalAmount);
    }

    [Fact]
    public async Task CreateAsync_decrements_stock()
    {
        var unitOfWork = SeedOrderDependencies();
        var service = new OrderService(unitOfWork);

        var result = await service.CreateAsync(NewOrder([new CreateOrderItemDto(1, 3)]));

        Assert.True(result.Succeeded);
        Assert.Equal(7, unitOfWork.ProductsRepo.Items[0].StockQuantity);
    }

    [Fact]
    public async Task CreateAsync_rethrows_concurrency_conflict()
    {
        var unitOfWork = SeedOrderDependencies();
        unitOfWork.CompleteException = new DbUpdateConcurrencyException("conflict");
        var service = new OrderService(unitOfWork);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
            service.CreateAsync(NewOrder([new CreateOrderItemDto(1, 1)])));
    }

    private static FakeUnitOfWork SeedOrderDependencies()
    {
        var unitOfWork = new FakeUnitOfWork();
        unitOfWork.UsersRepo.Items.Add(new User { UserId = 1, Email = "buyer@test.local" });
        unitOfWork.StoresRepo.Items.Add(new Store { StoreId = 1, StoreName = "Store" });
        unitOfWork.ProductsRepo.Items.Add(new Product { ProductId = 1, StoreId = 1, Name = "Product", BasePrice = 100, StockQuantity = 10 });
        return unitOfWork;
    }

    private static CreateOrderDto NewOrder(IReadOnlyList<CreateOrderItemDto> items) =>
        new(1, 1, null, "address", PaymentMethod.Cash, items);
}
