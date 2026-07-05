namespace Elattba.Core.DTOs;

public record OfferDto(
    int OfferId,
    int StoreId,
    string? StoreName,
    decimal DiscountPercentage,
    DateTime StartDate,
    DateTime EndDate,
    bool AppliesToAllProducts,
    DateTime CreatedAt,
    IReadOnlyList<int> ProductIds);

public record CreateOfferDto(
    int StoreId,
    decimal DiscountPercentage,
    DateTime StartDate,
    DateTime EndDate,
    bool AppliesToAllProducts,
    IReadOnlyList<int> ProductIds);

public record UpdateOfferDto(
    decimal DiscountPercentage,
    DateTime StartDate,
    DateTime EndDate,
    bool AppliesToAllProducts,
    IReadOnlyList<int> ProductIds);
