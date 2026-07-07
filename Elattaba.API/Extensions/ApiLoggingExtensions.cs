namespace Elattaba.API.Extensions
{
    public static class ApiLoggingExtensions
    {
        public static WebApplicationBuilder ConfigureApiLogging(this WebApplicationBuilder builder)
        {
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            return builder;
        }
    }
}
