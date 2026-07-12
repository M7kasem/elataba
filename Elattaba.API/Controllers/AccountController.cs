using Elattaba.API.Helper;
using Elattaba.API.Services;
using Elattba.Application.Auth;
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
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly UserManager<AppUser> _userManager;

    public AccountController(
        IUserProvisioningService userProvisioningService,
        UserManager<AppUser> userManager,
        IUnitOfWork unitOfWork,
        ITokenService tokenService)
    {
        _userProvisioningService = userProvisioningService;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _userProvisioningService.RegisterAsync(dto);
        if (!result.Succeeded || result.Data == null)
        {
            return this.ToActionResult(result);
        }

        var response = result.Data;
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
