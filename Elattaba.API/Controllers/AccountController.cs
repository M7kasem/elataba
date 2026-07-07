using ElAtaba.Domain.Entities;
using Elattaba.API.Helper;
using Elattba.Application.Auth;
using Elattba.Application.Users;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;
using Elattba.InfraStructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class AccountController : ControllerBase
{
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;

    public AccountController(
        UserManager<AppUser> userManager,
        IUnitOfWork unitOfWork,
        IPasswordHashingService passwordHashingService,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _passwordHashingService = passwordHashingService;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var governorate = await _unitOfWork.Governorates.GetByIdAsync(dto.GovernorateId);
        if (governorate == null)
        {
            return NotFound(new ResponseAPI(404, "Governorate not found."));
        }

        if (await _unitOfWork.Users.AnyAsync(user => user.Email == dto.Email) ||
            await _userManager.FindByEmailAsync(dto.Email) != null)
        {
            return BadRequest(new ResponseAPI(400, "Email is already registered."));
        }

        var domainUser = new User
        {
            Email = dto.Email,
            Phone = dto.Phone,
            Role = dto.Role,
            GovernorateId = dto.GovernorateId,
            City = dto.City,
            ShippingAddress = dto.ShippingAddress
        };
        domainUser.PasswordHash = _passwordHashingService.HashPassword(domainUser, dto.Password);

        await _unitOfWork.Users.AddAsync(domainUser);
        await _unitOfWork.CompleteAsync();

        var appUser = new AppUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            PhoneNumber = dto.Phone,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DomainUserId = domainUser.UserId
        };

        var createResult = await _userManager.CreateAsync(appUser, dto.Password);
        if (!createResult.Succeeded)
        {
            await _unitOfWork.Users.DeleteAsync(domainUser.UserId);
            await _unitOfWork.CompleteAsync();

            var message = string.Join(" ", createResult.Errors.Select(error => error.Description));
            return BadRequest(new ResponseAPI(400, message));
        }

        var roleResult = await _userManager.AddToRoleAsync(appUser, dto.Role.ToString());
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(appUser);
            await _unitOfWork.Users.DeleteAsync(domainUser.UserId);
            await _unitOfWork.CompleteAsync();

            var message = string.Join(" ", roleResult.Errors.Select(error => error.Description));
            return BadRequest(new ResponseAPI(400, message));
        }

        var response = await BuildAuthResponseAsync(appUser, dto.Role.ToString());
        WriteJwtCookie(response.AccessToken, response.ExpiresAtUtc);

        return Ok(new ResponseAPI(200, "Registered successfully", response));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var appUser = await _userManager.FindByEmailAsync(dto.Email);
        if (appUser == null || !await _userManager.CheckPasswordAsync(appUser, dto.Password))
        {
            return Unauthorized(new ResponseAPI(401, "Invalid email or password."));
        }

        var roles = await _userManager.GetRolesAsync(appUser);
        var role = roles.FirstOrDefault() ?? AuthConstants.BuyerRole;
        var response = await BuildAuthResponseAsync(appUser, role);
        WriteJwtCookie(response.AccessToken, response.ExpiresAtUtc);

        return Ok(new ResponseAPI(200, "Logged in successfully", response));
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(AuthConstants.JwtCookieName);
        return Ok(new ResponseAPI(200, "Logged out successfully"));
    }

    private async Task<AuthResponseDto> BuildAuthResponseAsync(AppUser appUser, string role)
    {
        var storeId = await ResolveStoreIdAsync(appUser.DomainUserId);
        appUser.StoreId = storeId;
        await _userManager.UpdateAsync(appUser);

        var token = _tokenService.CreateAccessToken(new TokenUserContext(
            appUser.Id,
            appUser.DomainUserId,
            appUser.Email ?? string.Empty,
            role,
            storeId));

        return new AuthResponseDto(
            token.AccessToken,
            token.ExpiresAtUtc,
            appUser.DomainUserId,
            appUser.Email ?? string.Empty,
            role,
            storeId);
    }

    private async Task<int?> ResolveStoreIdAsync(int domainUserId)
    {
        var stores = await _unitOfWork.Stores.ListAsync(
            store => store.OwnerId == domainUserId || store.ManagerId == domainUserId);

        return stores.FirstOrDefault()?.StoreId;
    }

    private void WriteJwtCookie(string token, DateTime expiresAtUtc)
    {
        Response.Cookies.Append(
            AuthConstants.JwtCookieName,
            token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = !HttpContext.Request.IsHttps ? false : true,
                SameSite = SameSiteMode.Strict,
                Expires = expiresAtUtc
            });
    }
}
