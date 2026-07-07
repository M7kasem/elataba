using ElAtaba.Domain.Enums;

namespace Elattba.Core.DTOs;

public record RegisterDto(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Phone,
    UserRole Role,
    int GovernorateId,
    string City,
    string ShippingAddress);

public record LoginDto(
    string Email,
    string Password);

public record AuthResponseDto(
    string AccessToken,
    DateTime ExpiresAtUtc,
    int UserId,
    string Email,
    string Role,
    int? StoreId);
