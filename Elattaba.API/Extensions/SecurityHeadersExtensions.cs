using Microsoft.AspNetCore.Builder;

namespace Elattaba.API.Extensions
{
    public static class SecurityHeadersExtensions
    {
        public static IApplicationBuilder UseApiSecurityHeaders(this IApplicationBuilder app)
        {
            var policies = new HeaderPolicyCollection()
                .AddDefaultApiSecurityHeaders();

            return app.UseSecurityHeaders(policies);
        }
    }
}
