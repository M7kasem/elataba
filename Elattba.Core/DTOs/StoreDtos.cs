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
    DateTime CreatedAt,
    IReadOnlyList<int>? ProductLineIds = null,
    IReadOnlyList<string>? ProductLineNames = null,
    string? LogoUrl = null);

public record CreateStoreDto(
    int OwnerId,
    int? ManagerId,
    int CategoryId,
    string StoreName,
    string Location,
    string Description,
    IReadOnlyList<int>? ProductLineIds = null);

public record UpdateStoreDto(
    int? ManagerId,
    int CategoryId,
    string StoreName,
    string Location,
    string Description,
    IReadOnlyList<int>? ProductLineIds = null);
