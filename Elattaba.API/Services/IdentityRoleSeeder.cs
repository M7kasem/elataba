using Elattba.Application.Auth;
using Microsoft.AspNetCore.Identity;

namespace Elattaba.API.Services;

public static class IdentityRoleSeeder
{
    private static readonly string[] Roles =
    [
        AuthConstants.AdminRole,
        AuthConstants.BuyerRole,
        AuthConstants.SellerRole,
        AuthConstants.StoreManagerRole
    ];

    public static async Task SeedIdentityRolesAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}
