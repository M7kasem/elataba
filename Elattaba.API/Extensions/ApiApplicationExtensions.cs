namespace Elattaba.API.Extensions
{
    public static class ApiApplicationExtensions
    {
        public static WebApplication UseApiMiddlewares(this WebApplication app)
        {
            app.UseExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseApiSecurityHeaders();
            app.UseStatusCodePagesWithReExecute("/errors/{0}");

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();
            app.UseApiRateLimiting();
            app.UseApiCors();
            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }

        public static WebApplication MapApiEndpoints(this WebApplication app)
        {
            app.MapControllers();

            return app;
        }
    }
}
