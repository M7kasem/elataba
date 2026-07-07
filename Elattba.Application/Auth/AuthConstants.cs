namespace Elattba.Application.Auth;

public static class AuthConstants
{
    public const string JwtCookieName = "jwt";

    public const string AdminOnlyPolicy = "AdminOnly";
    public const string SellerOnlyPolicy = "SellerOnly";
    public const string BuyerOnlyPolicy = "BuyerOnly";

    public const string AdminRole = "Admin";
    public const string BuyerRole = "Buyer";
    public const string SellerRole = "Seller";
    public const string StoreManagerRole = "StoreManager";
}
