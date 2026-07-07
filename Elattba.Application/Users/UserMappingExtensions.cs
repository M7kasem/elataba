using ElAtaba.Domain.Entities;
using Elattba.Core.DTOs;

namespace Elattba.Application.Users;

internal static class UserMappingExtensions
{
    public static UserDto ToUserDto(this User user) =>
        new(
            user.UserId,
            user.Email,
            user.Phone,
            user.Role,
            user.GovernorateId,
            user.Governorate?.Name,
            user.City,
            user.ShippingAddress,
            user.CreatedAt);
}
