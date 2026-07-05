namespace Elattba.Core.DTOs;

public record ProductDto(
    int ProductId,
    int StoreId,
    string? StoreName,
    int CategoryId,
    string? CategoryName,
    string Name,
    string Description,
    decimal BasePrice,
    decimal? OldPrice,
    decimal CurrentPrice,
    decimal? DiscountPercentage,
    bool HasActiveOffer,
    int StockQuantity,
    DateTime CreatedAt,
    IReadOnlyList<ProductImageDto> Images,
    IReadOnlyList<PricingTierDto> PricingTiers);

public record CreateProductDto(
    int StoreId,
    int CategoryId,
    string Name,
    string Description,
    decimal BasePrice,
    int StockQuantity,
    IReadOnlyList<CreateProductImageInputDto>? Images);

public record CreateProductWithOfferDto(
    int StoreId,
    int CategoryId,
    string Name,
    string Description,
    decimal BasePrice,
    int StockQuantity,
    decimal DiscountPercentage,
    DateTime StartDate,
    DateTime EndDate,
    IReadOnlyList<CreateProductImageInputDto>? Images);

public record UpdateProductDto(
    int CategoryId,
    string Name,
    string Description,
    decimal BasePrice,
    int StockQuantity,
    IReadOnlyList<CreateProductImageInputDto>? Images);
