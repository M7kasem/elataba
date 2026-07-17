using ElAtaba.Domain.Entities;
using Elattba.Application.Products;
using Elattba.Core.DTOs;

namespace Elattba.Tests;

public sealed class ProductQueryServiceTests
{
    [Fact]
    public async Task GetAllAsync_returns_default_paginated_products_sorted_by_name()
    {
        var unitOfWork = SeedProducts();
        var service = NewService(unitOfWork);

        var result = await service.GetAllAsync(new ProductParams { PageSize = 3 });

        Assert.True(result.Succeeded);
        Assert.Equal(5, result.Data!.Count);
        Assert.Equal(1, result.Data.PageNumber);
        Assert.Equal(3, result.Data.PageSize);
        Assert.Equal(["Alpha", "Alpha", "Bravo"], result.Data.Data.Select(product => product.Name));
    }

    [Fact]
    public async Task GetAllAsync_caps_page_size_and_defaults_bad_page_number()
    {
        var unitOfWork = SeedProducts();
        var service = NewService(unitOfWork);
        var productParams = new ProductParams
        {
            PageNumber = -2,
            PageSize = 100
        };

        var result = await service.GetAllAsync(productParams);

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.Data!.PageNumber);
        Assert.Equal(ProductParams.MaxPageSize, result.Data.PageSize);
    }

    [Fact]
    public async Task GetAllAsync_filters_by_category()
    {
        var unitOfWork = SeedProducts();
        var service = NewService(unitOfWork);

        var result = await service.GetAllAsync(new ProductParams { CategoryId = 2, PageSize = 6 });

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Data!.Count);
        Assert.All(result.Data.Data, product => Assert.Equal(2, product.CategoryId));
    }

    [Fact]
    public async Task GetAllAsync_sorts_by_price_desc()
    {
        var unitOfWork = SeedProducts();
        var service = NewService(unitOfWork);

        var result = await service.GetAllAsync(new ProductParams { Sort = "priceDesc", PageSize = 6 });

        Assert.True(result.Succeeded);
        Assert.Equal([300, 200, 150, 100, 50], result.Data!.Data.Select(product => product.BasePrice));
    }

    [Fact]
    public async Task GetAllAsync_searches_all_tokens_across_name_and_description()
    {
        var unitOfWork = SeedProducts();
        var service = NewService(unitOfWork);

        var result = await service.GetAllAsync(new ProductParams { Search = "cotton shirt", PageSize = 6 });

        Assert.True(result.Succeeded);
        var product = Assert.Single(result.Data!.Data);
        Assert.Equal("Bravo", product.Name);
    }

    [Fact]
    public async Task GetBestDealsAsync_excludes_expired_offers_and_out_of_stock_products()
    {
        var unitOfWork = SeedProducts();
        var now = DateTime.UtcNow;
        unitOfWork.ProductsRepo.Items.Single(product => product.ProductId == 2).StockQuantity = 0;
        unitOfWork.OffersRepo.Items.Add(new Offer
        {
            OfferId = 1,
            StoreId = 1,
            DiscountPercentage = 15,
            StartDate = now.AddDays(-1),
            EndDate = now.AddDays(2),
            OfferProducts = { new OfferProduct { OfferId = 1, ProductId = 1 } }
        });
        unitOfWork.OffersRepo.Items.Add(new Offer
        {
            OfferId = 2,
            StoreId = 1,
            DiscountPercentage = 80,
            StartDate = now.AddDays(-1),
            EndDate = now.AddDays(-1),
            AppliesToAllProducts = true
        });

        var service = NewService(unitOfWork);

        var result = await service.GetBestDealsAsync(10);

        Assert.True(result.Succeeded);
        var deals = result.Data!;
        Assert.Equal([1], deals.Select(product => product.ProductId));
        Assert.DoesNotContain(deals, product => product.ProductId == 2);
        Assert.DoesNotContain(deals, product => product.ProductId == 3);
    }

    [Fact]
    public async Task GetBestDealsAsync_uses_best_active_offer_for_price_and_end_date()
    {
        var unitOfWork = SeedProducts();
        var now = DateTime.UtcNow;
        unitOfWork.OffersRepo.Items.Add(new Offer
        {
            OfferId = 1,
            StoreId = 1,
            DiscountPercentage = 20,
            StartDate = now.AddDays(-1),
            EndDate = now.AddDays(2),
            AppliesToAllProducts = true
        });
        unitOfWork.OffersRepo.Items.Add(new Offer
        {
            OfferId = 2,
            StoreId = 1,
            DiscountPercentage = 30,
            StartDate = now.AddDays(-1),
            EndDate = now.AddDays(1),
            OfferProducts = { new OfferProduct { OfferId = 2, ProductId = 1 } }
        });
        var service = NewService(unitOfWork);

        var result = await service.GetBestDealsAsync(10);

        Assert.True(result.Succeeded);
        var deals = result.Data!;
        var productWithCompetingOffers = Assert.Single(deals, product => product.ProductId == 1);
        Assert.Equal(30, productWithCompetingOffers.DiscountPercentage);
        Assert.Equal(140, productWithCompetingOffers.CurrentPrice);
        Assert.Equal(now.AddDays(1), productWithCompetingOffers.OfferEndDate);
    }

    [Fact]
    public async Task GetBestDealsAsync_does_not_duplicate_products_when_storewide_and_specific_offers_overlap()
    {
        var unitOfWork = SeedProducts();
        var now = DateTime.UtcNow;
        unitOfWork.OffersRepo.Items.Add(new Offer
        {
            OfferId = 1,
            StoreId = 1,
            DiscountPercentage = 20,
            StartDate = now.AddDays(-1),
            EndDate = now.AddDays(2),
            AppliesToAllProducts = true
        });
        unitOfWork.OffersRepo.Items.Add(new Offer
        {
            OfferId = 2,
            StoreId = 1,
            DiscountPercentage = 35,
            StartDate = now.AddDays(-1),
            EndDate = now.AddDays(1),
            OfferProducts = { new OfferProduct { OfferId = 2, ProductId = 1 } }
        });
        unitOfWork.OffersRepo.Items.Add(new Offer
        {
            OfferId = 3,
            StoreId = 1,
            DiscountPercentage = 15,
            StartDate = now.AddDays(-1),
            EndDate = now.AddDays(1),
            OfferProducts = { new OfferProduct { OfferId = 3, ProductId = 2 } }
        });

        unitOfWork.OffersRepo.Items.Add(new Offer
        {
            OfferId = 4,
            StoreId = 1,
            DiscountPercentage = 80,
            StartDate = now.AddDays(-3),
            EndDate = now.AddDays(-1),
            AppliesToAllProducts = true
        });
        var service = NewService(unitOfWork);

        var result = await service.GetBestDealsAsync(10);

        Assert.True(result.Succeeded);
        var deals = result.Data!;
        Assert.Equal(deals.Count, deals.Select(product => product.ProductId).Distinct().Count());
        Assert.Equal([1, 2, 3, 4, 5], deals.Select(product => product.ProductId).OrderBy(productId => productId));
        var productWithOverlap = Assert.Single(deals, product => product.ProductId == 1);
        Assert.Equal(35, productWithOverlap.DiscountPercentage);
        Assert.DoesNotContain(deals, product => product.DiscountPercentage == 80);
    }

    [Fact]
    public async Task GetBestDealsAsync_caps_take_to_fifty()
    {
        var unitOfWork = new FakeUnitOfWork();
        var now = DateTime.UtcNow;
        unitOfWork.OffersRepo.Items.Add(new Offer
        {
            OfferId = 1,
            StoreId = 1,
            DiscountPercentage = 10,
            StartDate = now.AddDays(-1),
            EndDate = now.AddDays(1),
            AppliesToAllProducts = true
        });

        for (var productId = 1; productId <= 60; productId++)
        {
            unitOfWork.ProductsRepo.Items.Add(new Product
            {
                ProductId = productId,
                StoreId = 1,
                CategoryId = 1,
                Name = $"Product {productId:00}",
                Description = "Deal product",
                BasePrice = 100,
                StockQuantity = 5
            });
        }

        var service = NewService(unitOfWork);

        var result = await service.GetBestDealsAsync(500);

        Assert.True(result.Succeeded);
        Assert.Equal(50, result.Data!.Count);
    }

    private static FakeUnitOfWork SeedProducts()
    {
        var unitOfWork = new FakeUnitOfWork();
        unitOfWork.ProductsRepo.Items.Add(new Product
        {
            ProductId = 1,
            StoreId = 1,
            CategoryId = 1,
            Name = "Charlie",
            Description = "Leather jacket",
            BasePrice = 200,
            StockQuantity = 5
        });
        unitOfWork.ProductsRepo.Items.Add(new Product
        {
            ProductId = 2,
            StoreId = 1,
            CategoryId = 2,
            Name = "Alpha",
            Description = "Cotton trousers",
            BasePrice = 100,
            StockQuantity = 5
        });
        unitOfWork.ProductsRepo.Items.Add(new Product
        {
            ProductId = 3,
            StoreId = 1,
            CategoryId = 2,
            Name = "Bravo",
            Description = "Cotton formal shirt",
            BasePrice = 50,
            StockQuantity = 5
        });
        unitOfWork.ProductsRepo.Items.Add(new Product
        {
            ProductId = 4,
            StoreId = 1,
            CategoryId = 3,
            Name = "Delta",
            Description = "Running shoes",
            BasePrice = 300,
            StockQuantity = 5
        });
        unitOfWork.ProductsRepo.Items.Add(new Product
        {
            ProductId = 5,
            StoreId = 1,
            CategoryId = 1,
            Name = "Alpha",
            Description = "Another Alpha product",
            BasePrice = 150,
            StockQuantity = 5
        });
        return unitOfWork;
    }

    private static ProductService NewService(FakeUnitOfWork unitOfWork) =>
        new(unitOfWork, new FakeImageEmbeddingService(), new FakeImageManagementService());
}
