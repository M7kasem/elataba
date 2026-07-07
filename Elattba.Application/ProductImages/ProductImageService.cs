using ElAtaba.Domain.Entities;
using Elattba.Application.Common;
using Elattba.Application.Products;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;
using Elattba.Core.Services;

namespace Elattba.Application.ProductImages;

public sealed class ProductImageService : IProductImageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageManagementService _imageManagementService;
    private readonly IProductImageEmbeddingQueue _embeddingQueue;

    public ProductImageService(
        IUnitOfWork unitOfWork,
        IImageManagementService imageManagementService,
        IProductImageEmbeddingQueue embeddingQueue)
    {
        _unitOfWork = unitOfWork;
        _imageManagementService = imageManagementService;
        _embeddingQueue = embeddingQueue;
    }

    public async Task<ServiceResult<IReadOnlyList<ProductImageDto>>> GetAllAsync()
    {
        try
        {
            var images = await _unitOfWork.ProductImages.GetAllAsync();
            var data = images.Select(image => image.ToProductImageDto()).ToList();

            return new ServiceResult<IReadOnlyList<ProductImageDto>>(true, 200, "Product images retrieved successfully", data);
        }
        catch (Exception ex)
        {
            return Failure<IReadOnlyList<ProductImageDto>>(ex);
        }
    }

    public async Task<ServiceResult<ProductImageDto>> GetByIdAsync(int id)
    {
        try
        {
            var image = await _unitOfWork.ProductImages.GetByIdAsync(id);
            if (image == null)
            {
                return new ServiceResult<ProductImageDto>(false, 404, "Product image not found.");
            }

            return new ServiceResult<ProductImageDto>(true, 200, "Product image retrieved successfully", image.ToProductImageDto());
        }
        catch (Exception ex)
        {
            return Failure<ProductImageDto>(ex);
        }
    }

    public async Task<ServiceResult<ProductImageDto>> CreateAsync(CreateProductImageDto dto)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null)
            {
                return new ServiceResult<ProductImageDto>(false, 404, "Product not found.");
            }

            if (dto.IsPrimary)
            {
                await ClearPrimaryImagesAsync(dto.ProductId);
            }

            var image = new ProductImage
            {
                ProductId = dto.ProductId,
                ImageUrl = dto.ImageUrl,
                IsPrimary = dto.IsPrimary
            };

            await _unitOfWork.ProductImages.AddAsync(image);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult<ProductImageDto>(true, 201, "Product image created successfully", image.ToProductImageDto());
        }
        catch (Exception ex)
        {
            return Failure<ProductImageDto>(ex);
        }
    }

    public async Task<ServiceResult<ProductImageDto>> UploadAsync(UploadProductImageCommand command)
    {
        string? uploadedImageUrl = null;

        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(command.ProductId);
            if (product == null)
            {
                return new ServiceResult<ProductImageDto>(false, 404, "Product not found.");
            }

            uploadedImageUrl = await _imageManagementService.AddImageAsync(command.Image, "products");

            if (command.IsPrimary)
            {
                await ClearPrimaryImagesAsync(command.ProductId);
            }

            var productImage = new ProductImage
            {
                ProductId = command.ProductId,
                ImageUrl = uploadedImageUrl,
                IsPrimary = command.IsPrimary,
                EmbeddingVector = null
            };

            await _unitOfWork.ProductImages.AddAsync(productImage);
            await _unitOfWork.CompleteAsync();
            await _embeddingQueue.QueueAsync(productImage.ImageId);

            return new ServiceResult<ProductImageDto>(true, 201, "Product image uploaded successfully", productImage.ToProductImageDto());
        }
        catch (InvalidOperationException ex)
        {
            DeleteUploadedImage(uploadedImageUrl);
            return new ServiceResult<ProductImageDto>(false, 400, ex.Message);
        }
        catch (Exception ex)
        {
            DeleteUploadedImage(uploadedImageUrl);
            return Failure<ProductImageDto>(ex);
        }
    }

    public async Task<ServiceResult<IReadOnlyList<ProductImageDto>>> UploadManyAsync(UploadManyProductImagesCommand command)
    {
        var uploadedImageUrls = new List<string>();

        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(command.ProductId);
            if (product == null)
            {
                return new ServiceResult<IReadOnlyList<ProductImageDto>>(false, 404, "Product not found.");
            }

            if (command.Images.Count == 0)
            {
                return new ServiceResult<IReadOnlyList<ProductImageDto>>(false, 400, "At least one image file is required.");
            }

            if (command.PrimaryImageIndex < 0 || command.PrimaryImageIndex >= command.Images.Count)
            {
                return new ServiceResult<IReadOnlyList<ProductImageDto>>(false, 400, "Primary image index is out of range.");
            }

            await ClearPrimaryImagesAsync(command.ProductId);

            var productImages = new List<ProductImage>();
            for (var index = 0; index < command.Images.Count; index++)
            {
                var file = command.Images[index];
                var imageUrl = await _imageManagementService.AddImageAsync(file, "products");
                uploadedImageUrls.Add(imageUrl);

                var productImage = new ProductImage
                {
                    ProductId = command.ProductId,
                    ImageUrl = imageUrl,
                    IsPrimary = index == command.PrimaryImageIndex,
                    EmbeddingVector = null
                };

                productImages.Add(productImage);
                await _unitOfWork.ProductImages.AddAsync(productImage);
            }

            await _unitOfWork.CompleteAsync();
            foreach (var productImage in productImages)
            {
                await _embeddingQueue.QueueAsync(productImage.ImageId);
            }

            var data = productImages.Select(image => image.ToProductImageDto()).ToList();
            return new ServiceResult<IReadOnlyList<ProductImageDto>>(true, 200, "Product images uploaded successfully", data);
        }
        catch (InvalidOperationException ex)
        {
            DeleteUploadedImages(uploadedImageUrls);
            return new ServiceResult<IReadOnlyList<ProductImageDto>>(false, 400, ex.Message);
        }
        catch (Exception ex)
        {
            DeleteUploadedImages(uploadedImageUrls);
            return Failure<IReadOnlyList<ProductImageDto>>(ex);
        }
    }

    public async Task<ServiceResult<RebuildProductImageEmbeddingsResult>> RebuildEmbeddingsAsync(string webRootPath)
    {
        try
        {
            var images = await _unitOfWork.ProductImages.GetAllAsync();
            var queued = 0;
            var skipped = 0;

            foreach (var image in images)
            {
                var imagePath = GetLocalImagePath(webRootPath, image.ImageUrl);
                if (imagePath == null || !File.Exists(imagePath))
                {
                    skipped++;
                    continue;
                }

                await _embeddingQueue.QueueAsync(image.ImageId);
                queued++;
            }

            return new ServiceResult<RebuildProductImageEmbeddingsResult>(
                true,
                200,
                "Product image embeddings queued successfully",
                new RebuildProductImageEmbeddingsResult(queued, skipped));
        }
        catch (InvalidOperationException ex)
        {
            return new ServiceResult<RebuildProductImageEmbeddingsResult>(false, 400, ex.Message);
        }
        catch (Exception ex)
        {
            return Failure<RebuildProductImageEmbeddingsResult>(ex);
        }
    }

    public async Task<ServiceResult<ProductImageDto>> UpdateAsync(int id, UpdateProductImageDto dto)
    {
        try
        {
            var image = await _unitOfWork.ProductImages.GetByIdAsync(id);
            if (image == null)
            {
                return new ServiceResult<ProductImageDto>(false, 404, "Product image not found.");
            }

            var oldImageUrl = image.ImageUrl;

            if (dto.IsPrimary)
            {
                await ClearPrimaryImagesAsync(image.ProductId, image.ImageId);
            }

            image.ImageUrl = dto.ImageUrl;
            image.IsPrimary = dto.IsPrimary;
            if (!string.Equals(oldImageUrl, image.ImageUrl, StringComparison.OrdinalIgnoreCase))
            {
                image.EmbeddingVector = null;
            }

            await _unitOfWork.ProductImages.UpdateAsync(image);
            await _unitOfWork.CompleteAsync();

            if (!string.Equals(oldImageUrl, image.ImageUrl, StringComparison.OrdinalIgnoreCase))
            {
                _imageManagementService.DeleteImage(oldImageUrl);
            }

            if (!string.Equals(oldImageUrl, image.ImageUrl, StringComparison.OrdinalIgnoreCase))
            {
                await _embeddingQueue.QueueAsync(image.ImageId);
            }

            return new ServiceResult<ProductImageDto>(true, 200, "Product image updated successfully", image.ToProductImageDto());
        }
        catch (Exception ex)
        {
            return Failure<ProductImageDto>(ex);
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        try
        {
            var image = await _unitOfWork.ProductImages.GetByIdAsync(id);
            if (image == null)
            {
                return new ServiceResult(false, 404, "Product image not found.");
            }

            var imageUrl = image.ImageUrl;

            await _unitOfWork.ProductImages.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
            _imageManagementService.DeleteImage(imageUrl);

            return new ServiceResult(true, 200, "Product image deleted successfully");
        }
        catch (Exception)
        {
            return new ServiceResult(false, 500, "Unexpected server error.");
        }
    }

    private async Task ClearPrimaryImagesAsync(int productId, int? excludedImageId = null)
    {
        var primaryImages = await _unitOfWork.ProductImages.ListAsync(
            image =>
                image.ProductId == productId &&
                image.IsPrimary &&
                image.ImageId != excludedImageId,
            false);

        foreach (var image in primaryImages)
        {
            image.IsPrimary = false;
            await _unitOfWork.ProductImages.UpdateAsync(image);
        }
    }

    private void DeleteUploadedImage(string? imageUrl)
    {
        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            _imageManagementService.DeleteImage(imageUrl);
        }
    }

    private void DeleteUploadedImages(IEnumerable<string> imageUrls)
    {
        foreach (var imageUrl in imageUrls)
        {
            _imageManagementService.DeleteImage(imageUrl);
        }
    }

    private static string? GetLocalImagePath(string webRootPath, string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl) || Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
        {
            return null;
        }

        var relativePath = imageUrl.TrimStart('/', '\\')
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        var fullPath = Path.GetFullPath(Path.Combine(webRootPath, relativePath));
        var uploadsRoot = Path.GetFullPath(Path.Combine(webRootPath, "uploads"));

        return fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase)
            ? fullPath
            : null;
    }

    private static ServiceResult<T> Failure<T>(Exception ex) =>
        new(false, 500, "Unexpected server error.");
}
