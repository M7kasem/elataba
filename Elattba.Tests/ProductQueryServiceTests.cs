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

        var result = await service.GetAllAsync(new ProductParams());

        Assert.True(result.Succeeded);
        Assert.Equal(4, result.Data!.Count);
        Assert.Equal(1, result.Data.PageNumber);
        Assert.Equal(3, result.Data.PageSize);
        Assert.Equal(["Alpha", "Bravo", "Charlie"], result.Data.Data.Select(product => product.Name));
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
        Assert.Equal([300, 200, 100, 50], result.Data!.Data.Select(product => product.BasePrice));
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
        return unitOfWork;
    }

    private static ProductService NewService(FakeUnitOfWork unitOfWork) =>
        new(unitOfWork, new FakeImageEmbeddingService(), new FakeImageManagementService());
}
