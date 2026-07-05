namespace Elattba.Core.DTOs;

public record ReviewDto(
    int ReviewId,
    int OrderId,
    int StoreId,
    string? StoreName,
    int BuyerId,
    string? BuyerEmail,
    int Rating,
    string Comment,
    DateTime CreatedAt);

public record CreateReviewDto(
    int OrderId,
    int StoreId,
    int BuyerId,
    int Rating,
    string Comment);

public record UpdateReviewDto(
    int Rating,
    string Comment);
