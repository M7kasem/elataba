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
public class ProductImageController : BaseController
{
    private readonly IWebHostEnvironment _environment;
    private readonly IImageEmbeddingService _imageEmbeddingService;
    private readonly IImageManagementService _imageManagementService;

    public ProductImageController(
        IUnitOfWork unitOfWork,
        IImageManagementService imageManagementService,
        IImageEmbeddingService imageEmbeddingService,
        IWebHostEnvironment environment)
        : base(unitOfWork)
    {
        _environment = environment;
        _imageManagementService = imageManagementService;
        _imageEmbeddingService = imageEmbeddingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var images = await _unitOfWork.ProductImages.GetAllAsync();
            var data = images.Select(image => image.ToDto()).ToList();
            return Ok(new ResponseAPI(200, "Product images retrieved successfully", data));
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
            var image = await _unitOfWork.ProductImages.GetByIdAsync(id);
            if (image == null)
            {
                return NotFound(new ResponseAPI(404, "Product image not found."));
            }

            return Ok(new ResponseAPI(200, "Product image retrieved successfully", image.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductImageDto createProductImageDto)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(createProductImageDto.ProductId);
            if (product == null)
            {
                return NotFound(new ResponseAPI(404, "Product not found."));
            }

            if (createProductImageDto.IsPrimary)
            {
                await ClearPrimaryImagesAsync(createProductImageDto.ProductId);
            }

            var image = new ProductImage
            {
                ProductId = createProductImageDto.ProductId,
                ImageUrl = createProductImageDto.ImageUrl,
                IsPrimary = createProductImageDto.IsPrimary
            };

            await _unitOfWork.ProductImages.AddAsync(image);
            await _unitOfWork.CompleteAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = image.ImageId },
                new ResponseAPI(201, "Product image created successfully", image.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] int productId, [FromForm] IFormFile image, [FromForm] bool isPrimary = false)
    {
        string? uploadedImageUrl = null;

        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound(new ResponseAPI(404, "Product not found."));
            }

            if (image == null)
            {
                return BadRequest(new ResponseAPI(400, "Image file is required."));
            }

            await using var imageStream = image.OpenReadStream();
            uploadedImageUrl = await _imageManagementService.AddImageAsync(
                new ImageUploadFile(imageStream, image.FileName, image.ContentType, image.Length),
                "products");
            var embeddingVector = await TryGenerateEmbeddingVectorAsync(image);

            if (isPrimary)
            {
                await ClearPrimaryImagesAsync(productId);
            }

            var productImage = new ProductImage
            {
                ProductId = productId,
                ImageUrl = uploadedImageUrl,
                IsPrimary = isPrimary,
                EmbeddingVector = embeddingVector
            };

            await _unitOfWork.ProductImages.AddAsync(productImage);
            await _unitOfWork.CompleteAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = productImage.ImageId },
                new ResponseAPI(201, "Product image uploaded successfully", productImage.ToDto()));
        }
        catch (InvalidOperationException ex)
        {
            DeleteUploadedImage(uploadedImageUrl);
            return BadRequest(new ResponseAPI(400, ex.Message));
        }
        catch (Exception ex)
        {
            DeleteUploadedImage(uploadedImageUrl);
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPost("upload-many")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadMany(
        [FromForm] int productId,
        [FromForm] List<IFormFile> images,
        [FromForm] int primaryImageIndex = 0)
    {
        var uploadedImageUrls = new List<string>();

        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound(new ResponseAPI(404, "Product not found."));
            }

            if (images.Count == 0)
            {
                return BadRequest(new ResponseAPI(400, "At least one image file is required."));
            }

            if (primaryImageIndex < 0 || primaryImageIndex >= images.Count)
            {
                return BadRequest(new ResponseAPI(400, "Primary image index is out of range."));
            }

            await ClearPrimaryImagesAsync(productId);

            var productImages = new List<ProductImage>();
            for (var index = 0; index < images.Count; index++)
            {
                var file = images[index];
                await using var imageStream = file.OpenReadStream();
                var imageUrl = await _imageManagementService.AddImageAsync(
                    new ImageUploadFile(imageStream, file.FileName, file.ContentType, file.Length),
                    "products");
                var embeddingVector = await TryGenerateEmbeddingVectorAsync(file);

                uploadedImageUrls.Add(imageUrl);

                var productImage = new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = imageUrl,
                    IsPrimary = index == primaryImageIndex,
                    EmbeddingVector = embeddingVector
                };

                productImages.Add(productImage);
                await _unitOfWork.ProductImages.AddAsync(productImage);
            }

            await _unitOfWork.CompleteAsync();

            var data = productImages.Select(image => image.ToDto()).ToList();
            return Ok(new ResponseAPI(200, "Product images uploaded successfully", data));
        }
        catch (InvalidOperationException ex)
        {
            DeleteUploadedImages(uploadedImageUrls);
            return BadRequest(new ResponseAPI(400, ex.Message));
        }
        catch (Exception ex)
        {
            DeleteUploadedImages(uploadedImageUrls);
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPost("rebuild-embeddings")]
    public async Task<IActionResult> RebuildEmbeddings()
    {
        try
        {
            var images = await _unitOfWork.ProductImages.GetAllAsync();
            var updated = 0;
            var skipped = 0;
            string? rebuildError = null;

            foreach (var image in images)
            {
                var imagePath = GetLocalImagePath(image.ImageUrl);
                if (imagePath == null || !System.IO.File.Exists(imagePath))
                {
                    skipped++;
                    continue;
                }

                try
                {
                    await using var imageStream = System.IO.File.OpenRead(imagePath);
                    var embedding = await _imageEmbeddingService.GenerateEmbeddingAsync(imageStream);
                    image.EmbeddingVector = JsonSerializer.Serialize(embedding);

                    await _unitOfWork.ProductImages.UpdateAsync(image);
                    updated++;
                }
                catch (InvalidOperationException ex)
                {
                    rebuildError = ex.Message;
                    break;
                }
                catch
                {
                    skipped++;
                }
            }

            await _unitOfWork.CompleteAsync();

            if (rebuildError != null)
            {
                return BadRequest(new ResponseAPI(400, rebuildError));
            }

            return Ok(new ResponseAPI(200, "Product image embeddings rebuilt successfully", new
            {
                updated,
                skipped
            }));
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
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductImageDto updateProductImageDto)
    {
        try
        {
            var image = await _unitOfWork.ProductImages.GetByIdAsync(id);
            if (image == null)
            {
                return NotFound(new ResponseAPI(404, "Product image not found."));
            }

            var oldImageUrl = image.ImageUrl;

            if (updateProductImageDto.IsPrimary)
            {
                await ClearPrimaryImagesAsync(image.ProductId, image.ImageId);
            }

            image.ImageUrl = updateProductImageDto.ImageUrl;
            image.IsPrimary = updateProductImageDto.IsPrimary;
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

            return Ok(new ResponseAPI(200, "Product image updated successfully", image.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var image = await _unitOfWork.ProductImages.GetByIdAsync(id);
            if (image == null)
            {
                return NotFound(new ResponseAPI(404, "Product image not found."));
            }

            var imageUrl = image.ImageUrl;

            await _unitOfWork.ProductImages.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
            _imageManagementService.DeleteImage(imageUrl);

            return Ok(new ResponseAPI(200, "Product image deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    private async Task ClearPrimaryImagesAsync(int productId, int? excludedImageId = null)
    {
        var images = await _unitOfWork.ProductImages.GetAllAsync();
        var primaryImages = images.Where(image =>
            image.ProductId == productId &&
            image.IsPrimary &&
            image.ImageId != excludedImageId);

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

    private async Task<string?> TryGenerateEmbeddingVectorAsync(IFormFile image)
    {
        try
        {
            await using var imageStream = image.OpenReadStream();
            var embedding = await _imageEmbeddingService.GenerateEmbeddingAsync(imageStream);
            return JsonSerializer.Serialize(embedding);
        }
        catch
        {
            return null;
        }
    }

    private string? GetLocalImagePath(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl) || Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
        {
            return null;
        }

        var webRootPath = string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;

        var relativePath = imageUrl.TrimStart('/', '\\')
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        var fullPath = Path.GetFullPath(Path.Combine(webRootPath, relativePath));
        var uploadsRoot = Path.GetFullPath(Path.Combine(webRootPath, "uploads"));

        return fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase)
            ? fullPath
            : null;
    }
}
