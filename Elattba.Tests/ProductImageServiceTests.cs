using ElAtaba.Domain.Entities;
using Elattba.Application.ProductImages;
using Elattba.Core.DTOs;
using Elattba.Core.Services;

namespace Elattba.Tests;

public sealed class ProductImageServiceTests
{
    [Fact]
    public async Task CreateAsync_rejects_missing_product()
    {
        var unitOfWork = new FakeUnitOfWork();
        var service = NewService(unitOfWork);

        var result = await service.CreateAsync(new CreateProductImageDto(404, "/image.jpg", false));

        Assert.False(result.Succeeded);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_clears_existing_primary_image()
    {
        var unitOfWork = SeedProductImageDependencies();
        unitOfWork.ProductImagesRepo.Items.Add(new ProductImage { ImageId = 1, ProductId = 1, ImageUrl = "/old.jpg", IsPrimary = true });
        var service = NewService(unitOfWork);

        var result = await service.CreateAsync(new CreateProductImageDto(1, "/new.jpg", true));

        Assert.True(result.Succeeded);
        Assert.False(unitOfWork.ProductImagesRepo.Items.Single(image => image.ImageId == 1).IsPrimary);
        Assert.True(unitOfWork.ProductImagesRepo.Items.Single(image => image.ImageUrl == "/new.jpg").IsPrimary);
    }

    [Fact]
    public async Task UploadAsync_queues_embedding_for_uploaded_image()
    {
        var unitOfWork = SeedProductImageDependencies();
        var imageService = new FakeImageManagementService();
        imageService.UploadResults.Enqueue("/uploads/products/new.jpg");
        var queue = new FakeProductImageEmbeddingQueue();
        var service = NewService(unitOfWork, imageService, queue);

        var result = await service.UploadAsync(new UploadProductImageCommand(1, FakeFile(), true));

        Assert.True(result.Succeeded);
        var image = Assert.Single(unitOfWork.ProductImagesRepo.Items);
        Assert.Equal("/uploads/products/new.jpg", image.ImageUrl);
        Assert.Contains(image.ImageId, queue.QueuedImageIds);
    }

    [Fact]
    public async Task UploadManyAsync_rejects_primary_index_out_of_range()
    {
        var unitOfWork = SeedProductImageDependencies();
        var service = NewService(unitOfWork);

        var result = await service.UploadManyAsync(new UploadManyProductImagesCommand(1, [FakeFile()], 2));

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task UpdateAsync_deletes_old_image_and_queues_embedding_when_url_changes()
    {
        var unitOfWork = SeedProductImageDependencies();
        unitOfWork.ProductImagesRepo.Items.Add(new ProductImage { ImageId = 1, ProductId = 1, ImageUrl = "/old.jpg", IsPrimary = false, EmbeddingVector = "[1,2]" });
        var imageService = new FakeImageManagementService();
        var queue = new FakeProductImageEmbeddingQueue();
        var service = NewService(unitOfWork, imageService, queue);

        var result = await service.UpdateAsync(1, new UpdateProductImageDto("/new.jpg", true));

        Assert.True(result.Succeeded);
        Assert.Contains("/old.jpg", imageService.DeletedImages);
        Assert.Contains(1, queue.QueuedImageIds);
        Assert.Null(unitOfWork.ProductImagesRepo.Items[0].EmbeddingVector);
    }

    private static FakeUnitOfWork SeedProductImageDependencies()
    {
        var unitOfWork = new FakeUnitOfWork();
        unitOfWork.ProductsRepo.Items.Add(new Product { ProductId = 1, StoreId = 1, BasePrice = 10, StockQuantity = 1 });
        return unitOfWork;
    }

    private static ProductImageService NewService(
        FakeUnitOfWork unitOfWork,
        FakeImageManagementService? imageService = null,
        FakeProductImageEmbeddingQueue? queue = null) =>
        new(unitOfWork, imageService ?? new FakeImageManagementService(), queue ?? new FakeProductImageEmbeddingQueue());

    private static ImageUploadFile FakeFile() =>
        new(new MemoryStream([1, 2, 3]), "image.jpg", "image/jpeg", 3);
}
