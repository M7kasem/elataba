using System.Security.Claims;
using Elattba.Application.Auth;

namespace Elattaba.API.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public string? IdentityUserId =>
        GetClaim(ClaimTypes.NameIdentifier);

    public int? UserId =>
        int.TryParse(GetClaim("domain_user_id"), out var userId) ? userId : null;

    public int? StoreId =>
        int.TryParse(GetClaim("store_id"), out var storeId) ? storeId : null;

    public string? Role =>
        GetClaim(ClaimTypes.Role);

    private string? GetClaim(string claimType) =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(claimType);
}
