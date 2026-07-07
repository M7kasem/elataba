namespace Elattba.Application.Auth;

public sealed class JwtSettings
{
    public string Issuer { get; init; } = "El3ttba";
    public string Audience { get; init; } = "El3ttba.Client";
    public string Secret { get; init; } = string.Empty;
    public int AccessTokenMinutes { get; init; } = 60;
}
