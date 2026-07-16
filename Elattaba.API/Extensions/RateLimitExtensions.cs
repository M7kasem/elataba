using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Elattaba.API.Extensions
{
    public static class RateLimitExtensions
    {
        public static IServiceCollection AddApiRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 150       ,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            AutoReplenishment = true
                        }));
            });

            return services;
        }

        public static IApplicationBuilder UseApiRateLimiting(this IApplicationBuilder app)
        {
            return app.UseRateLimiter();
        }
    }
}
