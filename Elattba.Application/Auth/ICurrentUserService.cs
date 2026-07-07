namespace Elattba.Application.Auth;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    string? IdentityUserId { get; }
    int? UserId { get; }
    int? StoreId { get; }
    string? Role { get; }
}
