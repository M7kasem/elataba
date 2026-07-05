namespace Elattba.Core.DTOs;

public record StoreDto(
    int StoreId,
    int OwnerId,
    string? OwnerEmail,
    int? ManagerId,
    string? ManagerEmail,
    int CategoryId,
    string? CategoryName,
    string StoreName,
    string Location,
    string Description,
    decimal Rating,
    DateTime CreatedAt);

public record CreateStoreDto(
    int OwnerId,
    int? ManagerId,
    int CategoryId,
    string StoreName,
    string Location,
    string Description);

public record UpdateStoreDto(
    int? ManagerId,
    int CategoryId,
    string StoreName,
    string Location,
    string Description);
