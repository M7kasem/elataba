using ElAtaba.Domain.Entities;
using ElAtaba.Domain.Enums;
using Elattba.Application.Reviews;
using Elattba.Core.DTOs;

namespace Elattba.Tests;

public sealed class ReviewServiceTests
{
    [Fact]
    public async Task CreateAsync_rejects_order_that_is_not_delivered()
    {
        var unitOfWork = SeedReviewDependencies(OrderStatus.Pending);
        var service = new ReviewService(unitOfWork);

        var result = await service.CreateAsync(NewReview());

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_rejects_buyer_mismatch()
    {
        var unitOfWork = SeedReviewDependencies(OrderStatus.Delivered);
        var service = new ReviewService(unitOfWork);

        var result = await service.CreateAsync(NewReview(buyerId: 2));

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_rejects_store_mismatch()
    {
        var unitOfWork = SeedReviewDependencies(OrderStatus.Delivered);
        var service = new ReviewService(unitOfWork);

        var result = await service.CreateAsync(NewReview(storeId: 2));

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_rejects_duplicate_review_for_order()
    {
        var unitOfWork = SeedReviewDependencies(OrderStatus.Delivered);
        unitOfWork.ReviewsRepo.Items.Add(new Review { ReviewId = 1, OrderId = 1, StoreId = 1, BuyerId = 1, Rating = 5 });
        var service = new ReviewService(unitOfWork);

        var result = await service.CreateAsync(NewReview());

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
    }

    private static FakeUnitOfWork SeedReviewDependencies(OrderStatus status)
    {
        var unitOfWork = new FakeUnitOfWork();
        unitOfWork.OrdersRepo.Items.Add(new Order { OrderId = 1, BuyerId = 1, StoreId = 1, Status = status });
        return unitOfWork;
    }

    private static CreateReviewDto NewReview(int storeId = 1, int buyerId = 1) =>
        new(1, storeId, buyerId, 5, "Great");
}
