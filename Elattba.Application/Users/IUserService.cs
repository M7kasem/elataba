using Elattba.Application.Common;
using Elattba.Core.DTOs;

namespace Elattba.Application.Users;

public interface IUserService
{
    Task<ServiceResult<IReadOnlyList<UserDto>>> GetAllAsync();
    Task<ServiceResult<UserDto>> GetByIdAsync(int id);
    Task<ServiceResult<UserDto>> UpdateAsync(int id, UpdateUserDto dto);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult<string>> UploadProfilePictureAsync(int userId, Elattba.Core.Services.ImageUploadFile file);
}
