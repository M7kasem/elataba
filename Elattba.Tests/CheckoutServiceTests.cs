using ElAtaba.Domain.Entities;
using ElAtaba.Domain.Enums;
using Elattba.Application.Auth;
using Elattba.Application.Checkouts;
using Elattba.Core.DTOs;

namespace Elattba.Tests;

public sealed class CheckoutServiceTests
{
    [Fact]
    public async Task CreateAsync_rejects_checkout_without_items()
    {
        var unitOfWork = SeedCheckoutDependencies();
        var service = new CheckoutService(unitOfWork);

        var result = await service.CreateAsync(NewCheckout([]));

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_rejects_invalid_quantity()
    {
        var unitOfWork = SeedCheckoutDependencies();
        var service = new CheckoutService(unitOfWork);

        var result = await service.CreateAsync(NewCheckout([new CreateCheckoutItemDto(1, 0)]));

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_returns_not_found_for_missing_product()
    {
        var unitOfWork = SeedCheckoutDependencies();
        var service = new CheckoutService(unitOfWork);

        var result = await service.CreateAsync(NewCheckout([new CreateCheckoutItemDto(999, 1)]));

        Assert.False(result.Succeeded);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_rejects_insufficient_stock()
    {
        var unitOfWork = SeedCheckoutDependencies();
        unitOfWork.ProductsRepo.Items[0].StockQuantity = 1;
        var service = new CheckoutService(unitOfWork);

        var result = await service.CreateAsync(NewCheckout([new CreateCheckoutItemDto(1, 2)]));

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_rejects_buyer_mismatch()
    {
        var unitOfWork = SeedCheckoutDependencies();
        var currentUser = new FakeCurrentUserService { UserId = 2, Role = AuthConstants.BuyerRole };
        var service = new CheckoutService(unitOfWork, currentUser);

        var result = await service.CreateAsync(NewCheckout([new CreateCheckoutItemDto(1, 1)]));

        Assert.False(result.Succeeded);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_creates_one_order_for_products_from_one_store()
    {
        var unitOfWork = SeedCheckoutDependencies();
        var service = new CheckoutService(unitOfWork);

        var result = await service.CreateAsync(NewCheckout([
            new CreateCheckoutItemDto(1, 2),
            new CreateCheckoutItemDto(2, 1)
        ]));

        Assert.True(result.Succeeded);
        Assert.Equal(201, result.StatusCode);
        Assert.Single(unitOfWork.OrdersRepo.Items);
        Assert.Single(result.Data!.Orders);
        Assert.Equal(250, result.Data.TotalAmount);
        Assert.Equal(3, result.Data.Orders[0].ItemCount);
        Assert.Equal(1, unitOfWork.CompleteCalls);
    }

    [Fact]
    public async Task CreateAsync_creates_multiple_orders_for_products_from_different_stores()
    {
        var unitOfWork = SeedCheckoutDependencies();
        var service = new CheckoutService(unitOfWork);

        var result = await service.CreateAsync(NewCheckout([
            new CreateCheckoutItemDto(1, 2),
            new CreateCheckoutItemDto(3, 1)
        ]));

        Assert.True(result.Succeeded);
        Assert.Equal(2, unitOfWork.OrdersRepo.Items.Count);
        Assert.Equal(2, result.Data!.Orders.Count);
        Assert.Contains(result.Data.Orders, order => order.StoreId == 1 && order.TotalAmount == 200);
        Assert.Contains(result.Data.Orders, order => order.StoreId == 2 && order.TotalAmount == 200);
        Assert.Equal(400, result.Data.TotalAmount);
        Assert.Equal(1, unitOfWork.CompleteCalls);
    }

    [Fact]
    public async Task CreateAsync_uses_active_offer_pricing()
    {
        var unitOfWork = SeedCheckoutDependencies();
        unitOfWork.OffersRepo.Items.Add(new Offer
        {
            OfferId = 1,
            StoreId = 1,
            DiscountPercentage = 25,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            AppliesToAllProducts = true
        });
        var service = new CheckoutService(unitOfWork);

        var result = await service.CreateAsync(NewCheckout([new CreateCheckoutItemDto(1, 2)]));

        Assert.True(result.Succeeded);
        var order = Assert.Single(unitOfWork.OrdersRepo.Items);
        var item = Assert.Single(order.OrderItems);
        Assert.Equal(75, item.UnitPrice);
        Assert.Equal(150, item.Subtotal);
        Assert.Equal(150, order.TotalAmount);
        Assert.Equal(150, result.Data!.TotalAmount);
    }

    [Fact]
    public async Task CreateAsync_decrements_stock()
    {
        var unitOfWork = SeedCheckoutDependencies();
        var service = new CheckoutService(unitOfWork);

        var result = await service.CreateAsync(NewCheckout([new CreateCheckoutItemDto(1, 3)]));

        Assert.True(result.Succeeded);
        Assert.Equal(7, unitOfWork.ProductsRepo.Items[0].StockQuantity);
    }

    private static FakeUnitOfWork SeedCheckoutDependencies()
    {
        var unitOfWork = new FakeUnitOfWork();
        unitOfWork.UsersRepo.Items.Add(new User { UserId = 1, Email = "buyer@test.local" });
        unitOfWork.StoresRepo.Items.Add(new Store { StoreId = 1, StoreName = "Store A" });
        unitOfWork.StoresRepo.Items.Add(new Store { StoreId = 2, StoreName = "Store B" });
        unitOfWork.ProductsRepo.Items.Add(new Product { ProductId = 1, StoreId = 1, Name = "Product A", BasePrice = 100, StockQuantity = 10 });
        unitOfWork.ProductsRepo.Items.Add(new Product { ProductId = 2, StoreId = 1, Name = "Product B", BasePrice = 50, StockQuantity = 10 });
        unitOfWork.ProductsRepo.Items.Add(new Product { ProductId = 3, StoreId = 2, Name = "Product C", BasePrice = 200, StockQuantity = 10 });
        return unitOfWork;
    }

    private static CreateCheckoutDto NewCheckout(IReadOnlyList<CreateCheckoutItemDto> items) =>
        new(1, null, "address", PaymentMethod.Cash, items);
}
