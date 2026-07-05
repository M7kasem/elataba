namespace Elattba.Core.DTOs;

public record ProductImageSearchResultDto(
    ProductDto Product,
    ProductImageDto MatchedImage,
    double Score);
