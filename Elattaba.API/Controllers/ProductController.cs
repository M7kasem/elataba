using ElAtaba.Domain.Entities;
using Elattaba.API.Helper;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;
using Elattba.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController : BaseController
{
    private readonly IConfiguration _configuration;
    private readonly IImageEmbeddingService _imageEmbeddingService;
    private readonly IImageManagementService _imageManagementService;

    public ProductController(
        IUnitOfWork unitOfWork,
        IImageEmbeddingService imageEmbeddingService,
        IImageManagementService imageManagementService,
        IConfiguration configuration)
        : base(unitOfWork)
    {
        _imageEmbeddingService = imageEmbeddingService;
        _imageManagementService = imageManagementService;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var products = await _unitOfWork.Products.GetAllAsync(
                product => product.Store!,
                product => product.Category!,
                product => product.Images,
                product => product.PricingTiers);
            var activeOffers = await GetActiveOffersAsync();
            var data = products.Select(product => product.ToDto(activeOffers)).ToList();
            return Ok(new ResponseAPI(200, "Products retrieved successfully", data));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var products = await _unitOfWork.Products.GetAllAsync(
                product => product.Store!,
                product => product.Category!,
                product => product.Images,
                product => product.PricingTiers);
            var product = products.FirstOrDefault(item => item.ProductId == id);
            if (product == null)
            {
                return NotFound(new ResponseAPI(404, "Product not found."));
            }

            var activeOffers = await GetActiveOffersAsync();
            return Ok(new ResponseAPI(200, "Product retrieved successfully", product.ToDto(activeOffers)));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreateProductFormDto createProductDto, [FromForm] IFormFileCollection images)
    {
        var uploadedImageUrls = new List<string>();

        try
        {
            var store = await _unitOfWork.Stores.GetByIdAsync(createProductDto.StoreId);
            if (store == null)
            {
                return NotFound(new ResponseAPI(404, "Store not found."));
            }

            var category = await _unitOfWork.Categories.GetByIdAsync(createProductDto.CategoryId);
            if (category == null)
            {
                return NotFound(new ResponseAPI(404, "Category not found."));
            }

            var imageValidationError = ValidateProductImageFiles(images, createProductDto.PrimaryImageIndex);
            if (imageValidationError != null)
            {
                return BadRequest(imageValidationError);
            }

            uploadedImageUrls = (await UploadProductImagesAsync(images)).ToList();

            var product = new Product
            {
                StoreId = createProductDto.StoreId,
                CategoryId = createProductDto.CategoryId,
                Name = createProductDto.Name,
                Description = createProductDto.Description,
                BasePrice = createProductDto.BasePrice,
                StockQuantity = createProductDto.StockQuantity
            };

            AddProductImages(product, uploadedImageUrls, createProductDto.PrimaryImageIndex);

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.CompleteAsync();

            product.Store = store;
            product.Category = category;
            var activeOffers = await GetActiveOffersAsync();
            return CreatedAtAction(
                nameof(GetById),
                new { id = product.ProductId },
                new ResponseAPI(201, "Product created successfully", product.ToDto(activeOffers)));
        }
        catch (InvalidOperationException ex)
        {
            DeleteImageFiles(uploadedImageUrls);
            return BadRequest(new ResponseAPI(400, ex.Message));
        }
        catch (Exception ex)
        {
            DeleteImageFiles(uploadedImageUrls);
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPost("create-with-offer")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateWithOffer([FromForm] CreateProductWithOfferFormDto createProductDto, [FromForm] IFormFileCollection images)
    {
        var uploadedImageUrls = new List<string>();

        try
        {
            var store = await _unitOfWork.Stores.GetByIdAsync(createProductDto.StoreId);
            if (store == null)
            {
                return NotFound(new ResponseAPI(404, "Store not found."));
            }

            var category = await _unitOfWork.Categories.GetByIdAsync(createProductDto.CategoryId);
            if (category == null)
            {
                return NotFound(new ResponseAPI(404, "Category not found."));
            }

            var validationError = ValidateOffer(
                createProductDto.DiscountPercentage,
                createProductDto.StartDate,
                createProductDto.EndDate,
                appliesToAllProducts: false,
                productIds: [0]);
            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            var imageValidationError = ValidateProductImageFiles(images, createProductDto.PrimaryImageIndex);
            if (imageValidationError != null)
            {
                return BadRequest(imageValidationError);
            }

            uploadedImageUrls = (await UploadProductImagesAsync(images)).ToList();

            var product = new Product
            {
                StoreId = createProductDto.StoreId,
                CategoryId = createProductDto.CategoryId,
                Name = createProductDto.Name,
                Description = createProductDto.Description,
                BasePrice = createProductDto.BasePrice,
                StockQuantity = createProductDto.StockQuantity,
                HasOffer = true
            };

            var offer = new Offer
            {
                StoreId = createProductDto.StoreId,
                DiscountPercentage = createProductDto.DiscountPercentage,
                StartDate = createProductDto.StartDate,
                EndDate = createProductDto.EndDate,
                AppliesToAllProducts = false
            };

            product.OfferProducts.Add(new OfferProduct
            {
                Offer = offer
            });

            AddProductImages(product, uploadedImageUrls, createProductDto.PrimaryImageIndex);

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.CompleteAsync();

            product.Store = store;
            product.Category = category;
            var activeOffers = await GetActiveOffersAsync();
            return CreatedAtAction(
                nameof(GetById),
                new { id = product.ProductId },
                new ResponseAPI(201, "Product created with offer successfully", product.ToDto(activeOffers)));
        }
        catch (InvalidOperationException ex)
        {
            DeleteImageFiles(uploadedImageUrls);
            return BadRequest(new ResponseAPI(400, ex.Message));
        }
        catch (Exception ex)
        {
            DeleteImageFiles(uploadedImageUrls);
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPost("search-by-image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SearchByImage([FromForm] IFormFile image, [FromForm] int topK = 0)
    {
        try
        {
            if (image == null)
            {
                return BadRequest(new ResponseAPI(400, "Image file is required."));
            }

            if (topK <= 0)
            {
                topK = _configuration.GetValue("ImageSearch:DefaultTopK", 10);
            }

            topK = Math.Clamp(topK, 1, 50);

            await using var imageStream = image.OpenReadStream();
            var queryEmbedding = await _imageEmbeddingService.GenerateEmbeddingAsync(imageStream);

            var products = await _unitOfWork.Products.GetAllAsync(
                product => product.Store!,
                product => product.Category!,
                product => product.Images,
                product => product.PricingTiers);
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
                    item.Product.ToDto(activeOffers),
                    item.Image.ToDto(),
                    Math.Round(item.Score, 4)))
                .ToList();

            return Ok(new ResponseAPI(200, "Image search completed successfully", results));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ResponseAPI(400, ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateProductFormDto updateProductDto, [FromForm] IFormFileCollection images)
    {
        var uploadedImageUrls = new List<string>();
        var oldImageUrls = new List<string>();

        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound(new ResponseAPI(404, "Product not found."));
            }

            var category = await _unitOfWork.Categories.GetByIdAsync(updateProductDto.CategoryId);
            if (category == null)
            {
                return NotFound(new ResponseAPI(404, "Category not found."));
            }

            var existingImages = (await _unitOfWork.ProductImages.GetAllAsync())
                .Where(image => image.ProductId == id)
                .ToList();
            var hasNewImages = images.Count > 0;

            if (hasNewImages)
            {
                var imageValidationError = ValidateProductImageFiles(images, updateProductDto.PrimaryImageIndex);
                if (imageValidationError != null)
                {
                    return BadRequest(imageValidationError);
                }

                uploadedImageUrls = (await UploadProductImagesAsync(images)).ToList();
            }
            else if (existingImages.Count == 0)
            {
                return BadRequest(new ResponseAPI(400, "At least one product image is required."));
            }

            product.CategoryId = updateProductDto.CategoryId;
            product.Name = updateProductDto.Name;
            product.Description = updateProductDto.Description;
            product.BasePrice = updateProductDto.BasePrice;
            product.StockQuantity = updateProductDto.StockQuantity;

            if (hasNewImages)
            {
                oldImageUrls = existingImages.Select(image => image.ImageUrl).ToList();
                foreach (var image in existingImages)
                {
                    await _unitOfWork.ProductImages.DeleteAsync(image.ImageId);
                }

                AddProductImages(product, uploadedImageUrls, updateProductDto.PrimaryImageIndex);
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
                : existingImages;
            var activeOffers = await GetActiveOffersAsync();
            return Ok(new ResponseAPI(200, "Product updated successfully", product.ToDto(activeOffers)));
        }
        catch (InvalidOperationException ex)
        {
            DeleteImageFiles(uploadedImageUrls);
            return BadRequest(new ResponseAPI(400, ex.Message));
        }
        catch (Exception ex)
        {
            DeleteImageFiles(uploadedImageUrls);
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound(new ResponseAPI(404, "Product not found."));
            }

            var imageUrls = (await _unitOfWork.ProductImages.GetAllAsync())
                .Where(image => image.ProductId == id)
                .Select(image => image.ImageUrl)
                .ToList();

            await _unitOfWork.Products.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            DeleteImageFiles(imageUrls);

            return Ok(new ResponseAPI(200, "Product deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
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

    private async Task<IReadOnlyList<Offer>> GetActiveOffersAsync()
    {
        var now = DateTime.UtcNow;
        var offers = await _unitOfWork.Offers.GetAllAsync(offer => offer.OfferProducts);

        return offers
            .Where(offer => offer.StartDate <= now && offer.EndDate >= now)
            .ToList();
    }

    private static ResponseAPI? ValidateOffer(
        decimal discountPercentage,
        DateTime startDate,
        DateTime endDate,
        bool appliesToAllProducts,
        IReadOnlyList<int> productIds)
    {
        if (discountPercentage <= 0 || discountPercentage > 100)
        {
            return new ResponseAPI(400, "Discount percentage must be greater than 0 and at most 100.");
        }

        if (startDate >= endDate)
        {
            return new ResponseAPI(400, "Offer start date must be before end date.");
        }

        if (appliesToAllProducts && productIds.Count > 0)
        {
            return new ResponseAPI(400, "Product ids must be empty when the offer applies to all products.");
        }

        if (!appliesToAllProducts && productIds.Count == 0)
        {
            return new ResponseAPI(400, "Product ids are required when the offer does not apply to all products.");
        }

        return null;
    }

    private static ResponseAPI? ValidateProductImageFiles(IReadOnlyList<IFormFile>? images, int primaryImageIndex)
    {
        if (images == null || images.Count == 0)
        {
            return new ResponseAPI(400, "At least one product image is required.");
        }

        if (primaryImageIndex < 0 || primaryImageIndex >= images.Count)
        {
            return new ResponseAPI(400, "Primary image index is out of range.");
        }

        if (images.Any(image => image == null || image.Length == 0))
        {
            return new ResponseAPI(400, "Image file is required for every product image.");
        }

        return null;
    }

    private async Task<IReadOnlyList<string>> UploadProductImagesAsync(IReadOnlyList<IFormFile> images)
    {
        return await _imageManagementService.AddImagesAsync(
            images.Select(image => new ImageUploadFile(
                image.OpenReadStream(),
                image.FileName,
                image.ContentType,
                image.Length)),
            "products");
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

    public class CreateProductFormDto
    {
        public int StoreId { get; init; }
        public int CategoryId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public decimal BasePrice { get; init; }
        public int StockQuantity { get; init; }
        public int PrimaryImageIndex { get; init; }
    }

    public sealed class CreateProductWithOfferFormDto : CreateProductFormDto
    {
        public decimal DiscountPercentage { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
    }

    public sealed class UpdateProductFormDto
    {
        public int CategoryId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public decimal BasePrice { get; init; }
        public int StockQuantity { get; init; }
        public int PrimaryImageIndex { get; init; }
    }
}
