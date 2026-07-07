using System.Text.Json;
using ElAtaba.Domain.Entities;
using Elattba.Application.Auth;
using Elattba.Application.Common;
using Elattba.Application.Offers;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;
using Elattba.Core.Services;

namespace Elattba.Application.Products;

public sealed class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageEmbeddingService _imageEmbeddingService;
    private readonly IImageManagementService _imageManagementService;
    private readonly ICurrentUserService? _currentUserService;

    public ProductService(
        IUnitOfWork unitOfWork,
        IImageEmbeddingService imageEmbeddingService,
        IImageManagementService imageManagementService,
        ICurrentUserService? currentUserService = null)
    {
        _unitOfWork = unitOfWork;
        _imageEmbeddingService = imageEmbeddingService;
        _imageManagementService = imageManagementService;
        _currentUserService = currentUserService;
    }

    public async Task<ServiceResult<Pagination<ProductDto>>> GetAllAsync(ProductParams productParams)
    {
        try
        {
            var products = await _unitOfWork.Products.GetPagedAsync(productParams);
            var activeOffers = await GetActiveOffersAsync();
            var data = products.Items.Select(product => product.ToProductDto(activeOffers)).ToList();
            var pagination = new Pagination<ProductDto>(
                products.PageNumber,
                products.PageSize,
                products.Count,
                data);

            return new ServiceResult<Pagination<ProductDto>>(true, 200, "Products retrieved successfully", pagination);
        }
        catch (Exception ex)
        {
            return Failure<Pagination<ProductDto>>(ex);
        }
    }

    public async Task<ServiceResult<ProductDto>> GetByIdAsync(int id)
    {
        try
        {
            var product = await GetProductWithDetailsAsync(id);
            if (product == null)
            {
                return new ServiceResult<ProductDto>(false, 404, "Product not found.");
            }

            var activeOffers = await GetActiveOffersAsync();
            return new ServiceResult<ProductDto>(true, 200, "Product retrieved successfully", product.ToProductDto(activeOffers));
        }
        catch (Exception ex)
        {
            return Failure<ProductDto>(ex);
        }
    }

    public async Task<ServiceResult<ProductDto>> CreateAsync(CreateProductCommand command)
    {
        var uploadedImageUrls = new List<string>();

        try
        {
            var store = await _unitOfWork.Stores.GetByIdAsync(command.StoreId);
            if (store == null)
            {
                return new ServiceResult<ProductDto>(false, 404, "Store not found.");
            }

            var authorizationError = EnsureCanManageStore(store.StoreId);
            if (authorizationError != null)
            {
                return authorizationError;
            }

            var category = await _unitOfWork.Categories.GetByIdAsync(command.CategoryId);
            if (category == null)
            {
                return new ServiceResult<ProductDto>(false, 404, "Category not found.");
            }

            var imageValidationError = ValidateProductImageFiles(command.Images, command.PrimaryImageIndex);
            if (imageValidationError != null)
            {
                return imageValidationError;
            }

            uploadedImageUrls = (await UploadProductImagesAsync(command.Images)).ToList();

            var product = new Product
            {
                StoreId = command.StoreId,
                CategoryId = command.CategoryId,
                Name = command.Name,
                Description = command.Description,
                BasePrice = command.BasePrice,
                StockQuantity = command.StockQuantity
            };

            AddProductImages(product, uploadedImageUrls, command.PrimaryImageIndex);

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.CompleteAsync();

            product.Store = store;
            product.Category = category;
            var activeOffers = await GetActiveOffersAsync();

            return new ServiceResult<ProductDto>(
                true,
                201,
                "Product created successfully",
                product.ToProductDto(activeOffers));
        }
        catch (InvalidOperationException ex)
        {
            DeleteImageFiles(uploadedImageUrls);
            return new ServiceResult<ProductDto>(false, 400, ex.Message);
        }
        catch (Exception ex) when (IsConcurrencyException(ex))
        {
            DeleteImageFiles(uploadedImageUrls);
            throw;
        }
        catch (Exception ex)
        {
            DeleteImageFiles(uploadedImageUrls);
            return Failure<ProductDto>(ex);
        }
    }

    public async Task<ServiceResult<ProductDto>> CreateWithOfferAsync(CreateProductWithOfferCommand command)
    {
        var uploadedImageUrls = new List<string>();

        try
        {
            var store = await _unitOfWork.Stores.GetByIdAsync(command.StoreId);
            if (store == null)
            {
                return new ServiceResult<ProductDto>(false, 404, "Store not found.");
            }

            var authorizationError = EnsureCanManageStore(store.StoreId);
            if (authorizationError != null)
            {
                return authorizationError;
            }

            var category = await _unitOfWork.Categories.GetByIdAsync(command.CategoryId);
            if (category == null)
            {
                return new ServiceResult<ProductDto>(false, 404, "Category not found.");
            }

            var validationError = OfferBusinessRules.ValidateDiscountAndDates<ProductDto>(
                command.DiscountPercentage,
                command.StartDate,
                command.EndDate);
            if (validationError != null)
            {
                return validationError;
            }

            var overlapError = await ValidateNoStoreWideOfferOverlapAsync(
                command.StoreId,
                command.StartDate,
                command.EndDate);
            if (overlapError != null)
            {
                return overlapError;
            }

            var imageValidationError = ValidateProductImageFiles(command.Images, command.PrimaryImageIndex);
            if (imageValidationError != null)
            {
                return imageValidationError;
            }

            uploadedImageUrls = (await UploadProductImagesAsync(command.Images)).ToList();

            var product = new Product
            {
                StoreId = command.StoreId,
                CategoryId = command.CategoryId,
                Name = command.Name,
                Description = command.Description,
                BasePrice = command.BasePrice,
                StockQuantity = command.StockQuantity,
                HasOffer = true
            };

            var offer = new Offer
            {
                StoreId = command.StoreId,
                DiscountPercentage = command.DiscountPercentage,
                StartDate = command.StartDate,
                EndDate = command.EndDate,
                AppliesToAllProducts = false
            };

            product.OfferProducts.Add(new OfferProduct
            {
                Offer = offer
            });

            AddProductImages(product, uploadedImageUrls, command.PrimaryImageIndex);

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.CompleteAsync();

            product.Store = store;
            product.Category = category;
            var activeOffers = await GetActiveOffersAsync();

            return new ServiceResult<ProductDto>(
                true,
                201,
                "Product created with offer successfully",
                product.ToProductDto(activeOffers));
        }
        catch (InvalidOperationException ex)
        {
            DeleteImageFiles(uploadedImageUrls);
            return new ServiceResult<ProductDto>(false, 400, ex.Message);
        }
        catch (Exception ex) when (IsConcurrencyException(ex))
        {
            DeleteImageFiles(uploadedImageUrls);
            throw;
        }
        catch (Exception ex)
        {
            DeleteImageFiles(uploadedImageUrls);
            return Failure<ProductDto>(ex);
        }
    }

    public async Task<ServiceResult<IReadOnlyList<ProductImageSearchResultDto>>> SearchByImageAsync(SearchProductsByImageCommand command)
    {
        try
        {
            if (command.Image == null)
            {
                return new ServiceResult<IReadOnlyList<ProductImageSearchResultDto>>(false, 400, "Image file is required.");
            }

            var topK = command.TopK;
            if (topK <= 0)
            {
                topK = command.DefaultTopK;
            }

            topK = Math.Clamp(topK, 1, 50);

            var queryEmbedding = await _imageEmbeddingService.GenerateEmbeddingAsync(command.Image.Content);

            var products = await GetProductsWithDetailsAsync();
            var activeOffers = await GetActiveOffersAsync();

            var results = products
                .SelectMany(product => product.Images
                    .Select(productImage => new
                    {
                        Product = product,
                        Image = productImage,
                        Embedding = DeserializeEmbedding(productImage.EmbeddingVector)
                    })
                    .Where(item => item.Embedding != null && item.Embedding.Length == queryEmbedding.Length)
                    .Select(item => new
                    {
                        item.Product,
                        item.Image,
                        Score = CosineSimilarity(queryEmbedding, item.Embedding!)
                    }))
                .GroupBy(item => item.Product.ProductId)
                .Select(group => group.OrderByDescending(item => item.Score).First())
                .OrderByDescending(item => item.Score)
                .Take(topK)
                .Select(item => new ProductImageSearchResultDto(
                    item.Product.ToProductDto(activeOffers),
                    item.Image.ToProductImageDto(),
                    Math.Round(item.Score, 4)))
                .ToList();

            return new ServiceResult<IReadOnlyList<ProductImageSearchResultDto>>(
                true,
                200,
                "Image search completed successfully",
                results);
        }
        catch (InvalidOperationException ex)
        {
            return new ServiceResult<IReadOnlyList<ProductImageSearchResultDto>>(false, 400, ex.Message);
        }
        catch (Exception ex)
        {
            return Failure<IReadOnlyList<ProductImageSearchResultDto>>(ex);
        }
    }

    public async Task<ServiceResult<ProductDto>> UpdateAsync(int id, UpdateProductCommand command)
    {
        var uploadedImageUrls = new List<string>();
        var oldImageUrls = new List<string>();

        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                return new ServiceResult<ProductDto>(false, 404, "Product not found.");
            }

            var authorizationError = EnsureCanManageStore(product.StoreId);
            if (authorizationError != null)
            {
                return authorizationError;
            }

            var category = await _unitOfWork.Categories.GetByIdAsync(command.CategoryId);
            if (category == null)
            {
                return new ServiceResult<ProductDto>(false, 404, "Category not found.");
            }

            var existingImages = await _unitOfWork.ProductImages.ListAsync(image => image.ProductId == id);
            var hasNewImages = command.Images.Count > 0;

            if (hasNewImages)
            {
                var imageValidationError = ValidateProductImageFiles(command.Images, command.PrimaryImageIndex);
                if (imageValidationError != null)
                {
                    return imageValidationError;
                }

                uploadedImageUrls = (await UploadProductImagesAsync(command.Images)).ToList();
            }
            else if (existingImages.Count == 0)
            {
                return new ServiceResult<ProductDto>(false, 400, "At least one product image is required.");
            }

            product.CategoryId = command.CategoryId;
            product.Name = command.Name;
            product.Description = command.Description;
            product.BasePrice = command.BasePrice;
            product.StockQuantity = command.StockQuantity;

            if (hasNewImages)
            {
                oldImageUrls = existingImages.Select(image => image.ImageUrl).ToList();
                foreach (var image in existingImages)
                {
                    await _unitOfWork.ProductImages.DeleteAsync(image.ImageId);
                }

                AddProductImages(product, uploadedImageUrls, command.PrimaryImageIndex);
            }

            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.CompleteAsync();

            if (hasNewImages)
            {
                DeleteImageFiles(oldImageUrls);
            }

            product.Category = category;
            product.Images = hasNewImages
                ? product.Images
                : existingImages.ToList();
            var activeOffers = await GetActiveOffersAsync();

            return new ServiceResult<ProductDto>(
                true,
                200,
                "Product updated successfully",
                product.ToProductDto(activeOffers));
        }
        catch (InvalidOperationException ex)
        {
            DeleteImageFiles(uploadedImageUrls);
            return new ServiceResult<ProductDto>(false, 400, ex.Message);
        }
        catch (Exception ex) when (IsConcurrencyException(ex))
        {
            DeleteImageFiles(uploadedImageUrls);
            throw;
        }
        catch (Exception ex)
        {
            DeleteImageFiles(uploadedImageUrls);
            return Failure<ProductDto>(ex);
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                return new ServiceResult(false, 404, "Product not found.");
            }

            var authorizationError = EnsureCanManageStore(product.StoreId);
            if (authorizationError != null)
            {
                return authorizationError;
            }

            var imageUrls = (await _unitOfWork.ProductImages.ListAsync(image => image.ProductId == id))
                .Select(image => image.ImageUrl)
                .ToList();

            await _unitOfWork.Products.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            DeleteImageFiles(imageUrls);

            return new ServiceResult(true, 200, "Product deleted successfully");
        }
        catch (Exception)
        {
            return new ServiceResult(false, 500, "Unexpected server error.");
        }
    }

    private Task<IReadOnlyList<Product>> GetProductsWithDetailsAsync()
    {
        return _unitOfWork.Products.ListAsync(
            null,
            true,
            product => product.Store!,
            product => product.Category!,
            product => product.Images,
            product => product.PricingTiers);
    }

    private Task<Product?> GetProductWithDetailsAsync(int id)
    {
        return _unitOfWork.Products.GetFirstOrDefaultAsync(
            product => product.ProductId == id,
            true,
            product => product.Store!,
            product => product.Category!,
            product => product.Images,
            product => product.PricingTiers);
    }

    private async Task<IReadOnlyList<Offer>> GetActiveOffersAsync()
    {
        var now = DateTime.UtcNow;
        return await _unitOfWork.Offers.ListAsync(
            offer => offer.StartDate <= now && offer.EndDate >= now,
            true,
            offer => offer.OfferProducts);
    }

    private static ServiceResult<ProductDto>? ValidateProductImageFiles(IReadOnlyList<ImageUploadFile>? images, int primaryImageIndex)
    {
        if (images == null || images.Count == 0)
        {
            return new ServiceResult<ProductDto>(false, 400, "At least one product image is required.");
        }

        if (primaryImageIndex < 0 || primaryImageIndex >= images.Count)
        {
            return new ServiceResult<ProductDto>(false, 400, "Primary image index is out of range.");
        }

        if (images.Any(image => image == null || image.Length == 0))
        {
            return new ServiceResult<ProductDto>(false, 400, "Image file is required for every product image.");
        }

        return null;
    }

    private async Task<ServiceResult<ProductDto>?> ValidateNoStoreWideOfferOverlapAsync(
        int storeId,
        DateTime startDate,
        DateTime endDate)
    {
        var existingOffers = await _unitOfWork.Offers.ListAsync(
            offer => offer.StoreId == storeId && offer.StartDate < endDate && startDate < offer.EndDate,
            true,
            offer => offer.OfferProducts);

        var overlappingOffer = OfferBusinessRules.FindOverlappingOffer(
            existingOffers,
            startDate,
            endDate,
            appliesToAllProducts: false,
            productIds: []);

        return overlappingOffer == null
            ? null
            : new ServiceResult<ProductDto>(false, 400, OfferBusinessRules.BuildOverlapMessage(overlappingOffer));
    }

    private async Task<IReadOnlyList<string>> UploadProductImagesAsync(IReadOnlyList<ImageUploadFile> images)
    {
        return await _imageManagementService.AddImagesAsync(images, "products");
    }

    private static void AddProductImages(Product product, IReadOnlyList<string> imageUrls, int primaryImageIndex)
    {
        for (var index = 0; index < imageUrls.Count; index++)
        {
            product.Images.Add(new ProductImage
            {
                ImageUrl = imageUrls[index],
                IsPrimary = index == primaryImageIndex
            });
        }
    }

    private void DeleteImageFiles(IEnumerable<string> imageUrls)
    {
        foreach (var imageUrl in imageUrls)
        {
            _imageManagementService.DeleteImage(imageUrl);
        }
    }

    private static float[]? DeserializeEmbedding(string? embeddingVector)
    {
        if (string.IsNullOrWhiteSpace(embeddingVector))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<float[]>(embeddingVector);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static double CosineSimilarity(IReadOnlyList<float> firstVector, IReadOnlyList<float> secondVector)
    {
        var dotProduct = 0d;
        var firstMagnitude = 0d;
        var secondMagnitude = 0d;

        for (var index = 0; index < firstVector.Count; index++)
        {
            dotProduct += firstVector[index] * secondVector[index];
            firstMagnitude += firstVector[index] * firstVector[index];
            secondMagnitude += secondVector[index] * secondVector[index];
        }

        if (firstMagnitude == 0 || secondMagnitude == 0)
        {
            return 0;
        }

        return dotProduct / (Math.Sqrt(firstMagnitude) * Math.Sqrt(secondMagnitude));
    }

    private static ServiceResult<T> Failure<T>(Exception ex) =>
        new(false, 500, "Unexpected server error.");

    private ServiceResult<ProductDto>? EnsureCanManageStore(int storeId)
    {
        if (_currentUserService?.IsAuthenticated != true || _currentUserService.Role == AuthConstants.AdminRole)
        {
            return null;
        }

        return _currentUserService.StoreId == storeId
            ? null
            : new ServiceResult<ProductDto>(false, 403, "You are not allowed to manage this store.");
    }

    private ServiceResult? EnsureCanManageStoreForDelete(int storeId)
    {
        if (_currentUserService?.IsAuthenticated != true || _currentUserService.Role == AuthConstants.AdminRole)
        {
            return null;
        }

        return _currentUserService.StoreId == storeId
            ? null
            : new ServiceResult(false, 403, "You are not allowed to manage this store.");
    }

    private static bool IsConcurrencyException(Exception ex) =>
        ex.GetType().FullName == "Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException";
}
