using Elattaba.API.Extensions;
using Elattaba.API.Services;
namespace Elattaba.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.ConfigureApiLogging();
            builder.Services.AddApiServices(builder.Configuration);

            var app = builder.Build();

            app.Services.SeedIdentityRolesAsync().GetAwaiter().GetResult();
            app.UseApiMiddlewares();
            app.MapApiEndpoints();

            app.Run();
        }
    }

