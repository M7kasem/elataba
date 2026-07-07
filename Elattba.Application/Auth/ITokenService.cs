namespace Elattba.Application.Auth;

public interface ITokenService
{
    TokenResult CreateAccessToken(TokenUserContext user);
}

public sealed record TokenUserContext(
    string IdentityUserId,
    int DomainUserId,
    string Email,
    string Role,
    int? StoreId);

public sealed record TokenResult(
    string AccessToken,
    DateTime ExpiresAtUtc);
