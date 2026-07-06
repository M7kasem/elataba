using Elattba.Application.Common;
using Elattba.Core.DTOs;
using Elattba.Core.Services;

namespace Elattba.Application.ProductImages;

public interface IProductImageService
{
    Task<ServiceResult<IReadOnlyList<ProductImageDto>>> GetAllAsync();
    Task<ServiceResult<ProductImageDto>> GetByIdAsync(int id);
    Task<ServiceResult<ProductImageDto>> CreateAsync(CreateProductImageDto dto);
    Task<ServiceResult<ProductImageDto>> UploadAsync(UploadProductImageCommand command);
    Task<ServiceResult<IReadOnlyList<ProductImageDto>>> UploadManyAsync(UploadManyProductImagesCommand command);
    Task<ServiceResult<RebuildProductImageEmbeddingsResult>> RebuildEmbeddingsAsync(string webRootPath);
    Task<ServiceResult<ProductImageDto>> UpdateAsync(int id, UpdateProductImageDto dto);
    Task<ServiceResult> DeleteAsync(int id);
}

public sealed record UploadProductImageCommand(
    int ProductId,
    ImageUploadFile Image,
    bool IsPrimary);

public sealed record UploadManyProductImagesCommand(
    int ProductId,
    IReadOnlyList<ImageUploadFile> Images,
    int PrimaryImageIndex);

public sealed record RebuildProductImageEmbeddingsResult(
    int Queued,
    int Skipped);

public interface IProductImageEmbeddingQueue
{
    ValueTask QueueAsync(int productImageId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<int> ReadAllAsync(CancellationToken cancellationToken = default);
}
