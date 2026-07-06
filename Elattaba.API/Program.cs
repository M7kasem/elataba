using Elattaba.API.Extensions;
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

            app.UseApiMiddlewares();
            app.MapApiEndpoints();

            app.Run();
        }
    }
}
