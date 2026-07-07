using ElAtaba.Domain.Entities;
using Elattba.Application.Offers;
using Elattba.Core.DTOs;

namespace Elattba.Tests;

public sealed class OfferServiceTests
{
    [Fact]
    public async Task CreateAsync_rejects_product_offer_overlapping_same_product()
    {
        var now = DateTime.UtcNow;
        var unitOfWork = SeedOfferDependencies();
        unitOfWork.OffersRepo.Items.Add(new Offer
        {
            OfferId = 10,
            StoreId = 1,
            DiscountPercentage = 10,
            StartDate = now,
            EndDate = now.AddDays(3),
            AppliesToAllProducts = false,
            OfferProducts = { new OfferProduct { OfferId = 10, ProductId = 1 } }
        });
        var service = new OfferService(unitOfWork);

        var result = await service.CreateAsync(new CreateOfferDto(1, 20, now.AddDays(1), now.AddDays(4), false, [1]));

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_rejects_storewide_offer_overlapping_product_offer()
    {
        var now = DateTime.UtcNow;
        var unitOfWork = SeedOfferDependencies();
        unitOfWork.OffersRepo.Items.Add(new Offer
        {
            OfferId = 10,
            StoreId = 1,
            DiscountPercentage = 10,
            StartDate = now,
            EndDate = now.AddDays(3),
            AppliesToAllProducts = false,
            OfferProducts = { new OfferProduct { OfferId = 10, ProductId = 1 } }
        });
        var service = new OfferService(unitOfWork);

        var result = await service.CreateAsync(new CreateOfferDto(1, 20, now.AddDays(1), now.AddDays(4), true, []));

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_allows_non_overlapping_dates()
    {
        var now = DateTime.UtcNow;
        var unitOfWork = SeedOfferDependencies();
        unitOfWork.OffersRepo.Items.Add(new Offer
        {
            OfferId = 10,
            StoreId = 1,
            DiscountPercentage = 10,
            StartDate = now,
            EndDate = now.AddDays(1),
            AppliesToAllProducts = false,
            OfferProducts = { new OfferProduct { OfferId = 10, ProductId = 1 } }
        });
        var service = new OfferService(unitOfWork);

        var result = await service.CreateAsync(new CreateOfferDto(1, 20, now.AddDays(1), now.AddDays(2), false, [1]));

        Assert.True(result.Succeeded);
        Assert.Equal(201, result.StatusCode);
    }

    private static FakeUnitOfWork SeedOfferDependencies()
    {
        var unitOfWork = new FakeUnitOfWork();
        unitOfWork.StoresRepo.Items.Add(new Store { StoreId = 1, StoreName = "Store" });
        unitOfWork.ProductsRepo.Items.Add(new Product { ProductId = 1, StoreId = 1, BasePrice = 100, StockQuantity = 5 });
        return unitOfWork;
    }
}
