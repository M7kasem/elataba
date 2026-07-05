namespace Elattba.Core.DTOs;

public record ProductImageDto(
    int ImageId,
    int ProductId,
    string ImageUrl,
    bool IsPrimary,
    DateTime CreatedAt);

public record CreateProductImageDto(
    int ProductId,
    string ImageUrl,
    bool IsPrimary);

public record CreateProductImageInputDto(
    string ImageUrl,
    bool IsPrimary);

public record UpdateProductImageDto(
    string ImageUrl,
    bool IsPrimary);
