using Elattaba.API.Helper;
using Elattba.Application.Auth;
using Elattba.Application.Common;
using Elattba.Application.ProductImages;
using Elattba.Core.DTOs;
using Elattba.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductImageController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly IProductImageService _productImageService;

    public ProductImageController(
        IProductImageService productImageService,
        IWebHostEnvironment environment)
    {
        _environment = environment;
        _productImageService = productImageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _productImageService.GetAllAsync();
        return this.ToActionResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _productImageService.GetByIdAsync(id);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthConstants.SellerOnlyPolicy)]
    public async Task<IActionResult> Create([FromBody] CreateProductImageDto createProductImageDto)
    {
        var result = await _productImageService.CreateAsync(createProductImageDto);
        return ToCreatedActionResult(result);
    }

    [HttpPost("upload")]
    [Authorize(Policy = AuthConstants.SellerOnlyPolicy)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] UploadProductImageFormDto uploadDto)
    {
        if (uploadDto.Image == null)
        {
            return BadRequest(new ResponseAPI(400, "Image file is required."));
        }

        await using var imageStream = uploadDto.Image.OpenReadStream();
        var result = await _productImageService.UploadAsync(new UploadProductImageCommand(
            uploadDto.ProductId,
            new ImageUploadFile(
                imageStream,
                uploadDto.Image.FileName,
                uploadDto.Image.ContentType,
                uploadDto.Image.Length),
            uploadDto.IsPrimary));

        return ToCreatedActionResult(result);
    }

    [HttpPost("upload-many")]
    [Authorize(Policy = AuthConstants.SellerOnlyPolicy)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadMany([FromForm] UploadManyProductImagesFormDto uploadDto)
    {
        var imageFiles = ToUploadFiles(uploadDto.Images);

        try
        {
            var result = await _productImageService.UploadManyAsync(new UploadManyProductImagesCommand(
                uploadDto.ProductId,
                imageFiles,
                uploadDto.PrimaryImageIndex));

            return this.ToActionResult(result);
        }
        finally
        {
            DisposeUploadFiles(imageFiles);
        }
    }

    [HttpPost("rebuild-embeddings")]
    [Authorize(Policy = AuthConstants.AdminOnlyPolicy)]
    public async Task<IActionResult> RebuildEmbeddings()
    {
        var webRootPath = string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;

        var result = await _productImageService.RebuildEmbeddingsAsync(webRootPath);
        return this.ToActionResult(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AuthConstants.SellerOnlyPolicy)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductImageDto updateProductImageDto)
    {
        var result = await _productImageService.UpdateAsync(id, updateProductImageDto);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AuthConstants.SellerOnlyPolicy)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _productImageService.DeleteAsync(id);
        return this.ToActionResult(result);
    }

    private IActionResult ToCreatedActionResult(ServiceResult<ProductImageDto> result)
    {
        if (result.Succeeded && result.StatusCode == 201 && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data.ImageId },
                new ResponseAPI(result.StatusCode, result.Message, result.Data));
        }

        return this.ToActionResult(result);
    }

    private static IReadOnlyList<ImageUploadFile> ToUploadFiles(IEnumerable<IFormFile> images)
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

    public sealed class UploadProductImageFormDto
    {
        public int ProductId { get; init; }
        public IFormFile? Image { get; init; }
        public bool IsPrimary { get; init; }
    }

    public sealed class UploadManyProductImagesFormDto
    {
        public int ProductId { get; init; }
        public List<IFormFile> Images { get; init; } = [];
        public int PrimaryImageIndex { get; init; }
    }
}
