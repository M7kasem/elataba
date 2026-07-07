using Elattaba.API.Helper;
using Elattba.Application.Common;
using Elattba.Application.Products;
using Elattba.Core.DTOs;
using Elattba.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IProductService _productService;

    public ProductController(IProductService productService, IConfiguration configuration)
    {
        _productService = productService;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _productService.GetAllAsync();
        return ToActionResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _productService.GetByIdAsync(id);
        return ToActionResult(result);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreateProductFormDto createProductDto)
    {
        var imageFiles = ToUploadFiles(createProductDto.Images);

        try
        {
            var result = await _productService.CreateAsync(new CreateProductCommand(
                createProductDto.StoreId,
                createProductDto.CategoryId,
                createProductDto.Name,
                createProductDto.Description,
                createProductDto.BasePrice,
                createProductDto.StockQuantity,
                createProductDto.PrimaryImageIndex,
                imageFiles));

            return ToCreatedProductActionResult(result);
        }
        finally
        {
            DisposeUploadFiles(imageFiles);
        }
    }

    [HttpPost("create-with-offer")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateWithOffer([FromForm] CreateProductWithOfferFormDto createProductDto)
    {
        var imageFiles = ToUploadFiles(createProductDto.Images);

        try
        {
            var result = await _productService.CreateWithOfferAsync(new CreateProductWithOfferCommand(
                createProductDto.StoreId,
                createProductDto.CategoryId,
                createProductDto.Name,
                createProductDto.Description,
                createProductDto.BasePrice,
                createProductDto.StockQuantity,
                createProductDto.PrimaryImageIndex,
                imageFiles,
                createProductDto.DiscountPercentage,
                createProductDto.StartDate,
                createProductDto.EndDate));

            return ToCreatedProductActionResult(result);
        }
        finally
        {
            DisposeUploadFiles(imageFiles);
        }
    }

    [HttpPost("search-by-image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SearchByImage([FromForm] SearchByImageFormDto searchByImageDto)
    {
        await using var imageStream = searchByImageDto.Image?.OpenReadStream();
        var image = searchByImageDto.Image == null || imageStream == null
            ? null
            : new ImageUploadFile(
                imageStream,
                searchByImageDto.Image.FileName,
                searchByImageDto.Image.ContentType,
                searchByImageDto.Image.Length);

        var result = await _productService.SearchByImageAsync(new SearchProductsByImageCommand(
            image,
            searchByImageDto.TopK,
            _configuration.GetValue("ImageSearch:DefaultTopK", 10)));

        return ToActionResult(result);
    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateProductFormDto updateProductDto)
    {
        var imageFiles = ToUploadFiles(updateProductDto.Images);

        try
        {
            var result = await _productService.UpdateAsync(id, new UpdateProductCommand(
                updateProductDto.CategoryId,
                updateProductDto.Name,
                updateProductDto.Description,
                updateProductDto.BasePrice,
                updateProductDto.StockQuantity,
                updateProductDto.PrimaryImageIndex,
                imageFiles));

            return ToActionResult(result);
        }
        finally
        {
            DisposeUploadFiles(imageFiles);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _productService.DeleteAsync(id);
        return ToActionResult(result);
    }

    private IActionResult ToCreatedProductActionResult(ServiceResult<ProductDto> result)
    {
        if (result.Succeeded && result.StatusCode == 201 && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data.ProductId },
                new ResponseAPI(result.StatusCode, result.Message, result.Data));
        }

        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        var response = new ResponseAPI(result.StatusCode, result.Message, result.Data);

        return result.StatusCode switch
        {
            200 => Ok(response),
            404 => NotFound(response),
            >= 500 => Problem(statusCode: result.StatusCode, title: "Internal Server Error", detail: result.Message),
            _ => StatusCode(result.StatusCode, response)
        };
    }

    private IActionResult ToActionResult(ServiceResult result)
    {
        var response = new ResponseAPI(result.StatusCode, result.Message);

        return result.StatusCode switch
        {
            200 => Ok(response),
            204 => NoContent(),
            404 => NotFound(response),
            >= 500 => Problem(statusCode: result.StatusCode, title: "Internal Server Error", detail: result.Message),
            _ => StatusCode(result.StatusCode, response)
        };
    }

    private static IReadOnlyList<ImageUploadFile> ToUploadFiles(IReadOnlyList<IFormFile> images)
    {
        return images
            .Select(image => new ImageUploadFile(
                image.OpenReadStream(),
                image.FileName,
                image.ContentType,
                image.Length))
            .ToList();
    }

    private static void DisposeUploadFiles(IEnumerable<ImageUploadFile> imageFiles)
    {
        foreach (var imageFile in imageFiles)
        {
            imageFile.Content.Dispose();
        }
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
        public IFormFileCollection Images { get; init; } = new FormFileCollection();
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
        public IFormFileCollection Images { get; init; } = new FormFileCollection();
    }

    public sealed class SearchByImageFormDto
    {
        public IFormFile? Image { get; init; }
        public int TopK { get; init; }
    }
}
