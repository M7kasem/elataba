using ElAtaba.Domain.Enums;

namespace Elattba.Core.DTOs;

public record UserDto(
    int UserId,
    string Email,
    string? Phone,
    UserRole Role,
    int GovernorateId,
    string? GovernorateName,
    string City,
    string ShippingAddress,
    DateTime CreatedAt);

public record CreateUserDto(
    string Email,
    string Password,
    string? Phone,
    UserRole Role,
    int GovernorateId,
    string City,
    string ShippingAddress);

public record UpdateUserDto(
    string Email,
    string? Phone,
    UserRole Role,
    int GovernorateId,
    string City,
    string ShippingAddress);
