using ElAtaba.Domain.Entities;
using Elattba.Application.Auth;
using Elattba.Application.Common;
using Elattba.Core.DTOs;
using Elattba.InfraStructure.Data;
using Elattba.InfraStructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Elattaba.API.Services;

public interface IUserProvisioningService
{
    Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto dto);
    Task<ServiceResult<UserDto>> CreateByAdminAsync(CreateUserDto dto);
}

public sealed class UserProvisioningService : IUserProvisioningService
{
    private readonly AppIdentityDbContext _identityDbContext;
    private readonly El3atbaDbContext _domainDbContext;
    private readonly ITokenService _tokenService;
    private readonly UserManager<AppUser> _userManager;

    public UserProvisioningService(
        El3atbaDbContext domainDbContext,
        AppIdentityDbContext identityDbContext,
        UserManager<AppUser> userManager,
        ITokenService tokenService)
    {
        _domainDbContext = domainDbContext;
        _identityDbContext = identityDbContext;
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        var result = await ProvisionUserAsync(new UserProvisioningInput(
            dto.Email,
            dto.Password,
            dto.FirstName,
            dto.LastName,
            dto.Phone,
            dto.Role,
            dto.GovernorateId,
            dto.City,
            dto.ShippingAddress));

        if (!result.Succeeded || result.Data == null)
        {
            return new ServiceResult<AuthResponseDto>(result.Succeeded, result.StatusCode, result.Message);
        }

        var appUser = result.Data.AppUser;
        var token = _tokenService.CreateAccessToken(new TokenUserContext(
            appUser.Id,
            result.Data.User.UserId,
            appUser.Email ?? string.Empty,
            dto.Role.ToString(),
            StoreId: null));

        return new ServiceResult<AuthResponseDto>(
            true,
            200,
            "Registered successfully",
            new AuthResponseDto(
                token.AccessToken,
                token.ExpiresAtUtc,
                result.Data.User.UserId,
                appUser.Email ?? string.Empty,
                dto.Role.ToString(),
                StoreId: null));
    }

    public async Task<ServiceResult<UserDto>> CreateByAdminAsync(CreateUserDto dto)
    {
        var result = await ProvisionUserAsync(new UserProvisioningInput(
            dto.Email,
            dto.Password,
            dto.FirstName,
            dto.LastName,
            dto.Phone,
            dto.Role,
            dto.GovernorateId,
            dto.City,
            dto.ShippingAddress));

        if (!result.Succeeded || result.Data == null)
        {
            return new ServiceResult<UserDto>(result.Succeeded, result.StatusCode, result.Message);
        }

        var user = result.Data.User;
        var governorate = result.Data.Governorate;
        var userDto = new UserDto(
            user.UserId,
            user.Email,
            user.Phone,
            user.Role,
            user.GovernorateId,
            governorate.Name,
            user.City,
            user.ShippingAddress,
            user.CreatedAt);

        return new ServiceResult<UserDto>(true, 201, "User created successfully", userDto);
    }

    private async Task<ServiceResult<UserProvisioningResult>> ProvisionUserAsync(UserProvisioningInput input)
    {
        var governorate = await _domainDbContext.Governorates.FindAsync(input.GovernorateId);
        if (governorate == null)
        {
            return new ServiceResult<UserProvisioningResult>(false, 404, "Governorate not found.");
        }

        if (await _domainDbContext.Users.AnyAsync(user => user.Email == input.Email) ||
            await _userManager.FindByEmailAsync(input.Email) != null)
        {
            return new ServiceResult<UserProvisioningResult>(false, 400, "Email is already registered.");
        }

        await using var transaction = await _domainDbContext.Database.BeginTransactionAsync();
        await _identityDbContext.Database.UseTransactionAsync(transaction.GetDbTransaction());

        var domainUser = new User
        {
            Email = input.Email,
            Phone = input.Phone,
            Role = input.Role,
            GovernorateId = input.GovernorateId,
            City = input.City,
            ShippingAddress = input.ShippingAddress
        };

        _domainDbContext.Users.Add(domainUser);
        await _domainDbContext.SaveChangesAsync();

        var appUser = new AppUser
        {
            UserName = input.Email,
            Email = input.Email,
            PhoneNumber = input.Phone,
            FirstName = input.FirstName,
            LastName = input.LastName,
            DomainUserId = domainUser.UserId
        };

        var createResult = await _userManager.CreateAsync(appUser, input.Password);
        if (!createResult.Succeeded)
        {
            await transaction.RollbackAsync();
            return new ServiceResult<UserProvisioningResult>(false, 400, BuildIdentityErrorMessage(createResult));
        }

        var roleResult = await _userManager.AddToRoleAsync(appUser, input.Role.ToString());
        if (!roleResult.Succeeded)
        {
            await transaction.RollbackAsync();
            return new ServiceResult<UserProvisioningResult>(false, 400, BuildIdentityErrorMessage(roleResult));
        }

        await transaction.CommitAsync();

        return new ServiceResult<UserProvisioningResult>(
            true,
            201,
            "User created successfully",
            new UserProvisioningResult(domainUser, appUser, governorate));
    }

    private static string BuildIdentityErrorMessage(IdentityResult result) =>
        string.Join(" ", result.Errors.Select(error => error.Description));

    private sealed record UserProvisioningInput(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string? Phone,
        ElAtaba.Domain.Enums.UserRole Role,
        int GovernorateId,
        string City,
        string ShippingAddress);

    private sealed record UserProvisioningResult(
        User User,
        AppUser AppUser,
        ElAtaba.Domain.Entities.Governorate Governorate);
}
