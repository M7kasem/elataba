using Elattba.Application.Common;
using Elattba.Core.DTOs;
using Elattba.Core.Services;

namespace Elattba.Application.Products;

public interface IProductService
{
    Task<ServiceResult<IReadOnlyList<ProductDto>>> GetAllAsync();
    Task<ServiceResult<ProductDto>> GetByIdAsync(int id);
    Task<ServiceResult<ProductDto>> CreateAsync(CreateProductCommand command);
    Task<ServiceResult<ProductDto>> CreateWithOfferAsync(CreateProductWithOfferCommand command);
    Task<ServiceResult<IReadOnlyList<ProductImageSearchResultDto>>> SearchByImageAsync(SearchProductsByImageCommand command);
    Task<ServiceResult<ProductDto>> UpdateAsync(int id, UpdateProductCommand command);
    Task<ServiceResult> DeleteAsync(int id);
}

public sealed record CreateProductCommand(
    int StoreId,
    int CategoryId,
    string Name,
    string Description,
    decimal BasePrice,
    int StockQuantity,
    int PrimaryImageIndex,
    IReadOnlyList<ImageUploadFile> Images);

public sealed record CreateProductWithOfferCommand(
    int StoreId,
    int CategoryId,
    string Name,
    string Description,
    decimal BasePrice,
    int StockQuantity,
    int PrimaryImageIndex,
    IReadOnlyList<ImageUploadFile> Images,
    decimal DiscountPercentage,
    DateTime StartDate,
    DateTime EndDate);

public sealed record UpdateProductCommand(
    int CategoryId,
    string Name,
    string Description,
    decimal BasePrice,
    int StockQuantity,
    int PrimaryImageIndex,
    IReadOnlyList<ImageUploadFile> Images);

public sealed record SearchProductsByImageCommand(
    ImageUploadFile? Image,
    int TopK,
    int DefaultTopK);
