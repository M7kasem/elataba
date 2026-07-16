using ElAtaba.Domain.Entities;
using Elattba.Application.Common;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;
using Elattba.Core.Services;

namespace Elattba.Application.Users;

public sealed class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageManagementService _imageManagementService;

    public UserService(
        IUnitOfWork unitOfWork,
        IImageManagementService imageManagementService)
    {
        _unitOfWork = unitOfWork;
        _imageManagementService = imageManagementService;
    }

    public async Task<ServiceResult<IReadOnlyList<UserDto>>> GetAllAsync()
    {
        try
        {
            var users = await _unitOfWork.Users.ListAsync(
                null,
                true,
                user => user.Governorate!);
            var data = users.Select(user => user.ToUserDto()).ToList();

            return new ServiceResult<IReadOnlyList<UserDto>>(true, 200, "Users retrieved successfully", data);
        }
        catch (Exception ex)
        {
            return Failure<IReadOnlyList<UserDto>>(ex);
        }
    }

    public async Task<ServiceResult<UserDto>> GetByIdAsync(int id)
    {
        try
        {
            var user = await GetUserWithGovernorateAsync(id, disableTracking: true);
            if (user == null)
            {
                return new ServiceResult<UserDto>(false, 404, "User not found.");
            }

            return new ServiceResult<UserDto>(true, 200, "User retrieved successfully", user.ToUserDto());
        }
        catch (Exception ex)
        {
            return Failure<UserDto>(ex);
        }
    }

    public async Task<ServiceResult<UserDto>> UpdateAsync(int id, UpdateUserDto dto)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return new ServiceResult<UserDto>(false, 404, "User not found.");
            }

            var governorate = await _unitOfWork.Governorates.GetByIdAsync(dto.GovernorateId);
            if (governorate == null)
            {
                return new ServiceResult<UserDto>(false, 404, "Governorate not found.");
            }

            if (await _unitOfWork.Users.AnyAsync(existingUser => existingUser.Email == dto.Email && existingUser.UserId != id))
            {
                return new ServiceResult<UserDto>(false, 400, "Email is already registered.");
            }

            user.Email = dto.Email;
            user.Phone = dto.Phone;
            user.Role = dto.Role;
            user.GovernorateId = dto.GovernorateId;
            user.City = dto.City;
            user.ShippingAddress = dto.ShippingAddress;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            user.Governorate = governorate;
            return new ServiceResult<UserDto>(true, 200, "User updated successfully", user.ToUserDto());
        }
        catch (Exception ex)
        {
            return Failure<UserDto>(ex);
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return new ServiceResult(false, 404, "User not found.");
            }

            await _unitOfWork.Users.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult(true, 200, "User deleted successfully");
        }
        catch (Exception)
        {
            return new ServiceResult(false, 500, "Unexpected server error.");
        }
    }

    private Task<User?> GetUserWithGovernorateAsync(int id, bool disableTracking)
    {
        return _unitOfWork.Users.GetFirstOrDefaultAsync(
            user => user.UserId == id,
            disableTracking,
            user => user.Governorate!);
    }

    private static ServiceResult<T> Failure<T>(Exception ex) =>
        new(false, 500, "Unexpected server error.");

    public async Task<ServiceResult<string>> UploadProfilePictureAsync(int userId, ImageUploadFile file)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return new ServiceResult<string>(false, 404, "User not found.");
            }

            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                _imageManagementService.DeleteImage(user.ProfilePictureUrl);
            }

            var imageUrl = await _imageManagementService.AddImageAsync(file, "users");

            user.ProfilePictureUrl = imageUrl;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult<string>(true, 200, "Profile picture uploaded successfully.", imageUrl);
        }
        catch (Exception ex)
        {
            return Failure<string>(ex);
        }
    }
}
