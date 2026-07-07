namespace Elattaba.API.Extensions;

public static class CorsExtensions
{
    public const string CorsPolicyName = "CorsPolicy";

    public static IServiceCollection AddApiCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["https://localhost:4200", "http://localhost:4200", "https://localhost:5173", "http://localhost:5173"];

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }

    public static IApplicationBuilder UseApiCors(this IApplicationBuilder app) =>
        app.UseCors(CorsPolicyName);
}
