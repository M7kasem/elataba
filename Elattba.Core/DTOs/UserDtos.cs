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
    DateTime CreatedAt,
    string FirstName = "",
    string LastName = "",
    string? ProfilePictureUrl = null);

public record CreateUserDto(
    string Email,
    string Password,
    string FirstName,
    string LastName,
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
    string ShippingAddress,
    string FirstName = "",
    string LastName = "");
