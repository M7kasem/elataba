using Elattaba.API.Helper;
using System.Net;

namespace Elattaba.API.MiddleWare
{
    public class ExceptionMiddleWare
    {
        private readonly IHostEnvironment _environment;
        private readonly RequestDelegate _next;

        public ExceptionMiddleWare(RequestDelegate next, IHostEnvironment environment)
        {
            _next = next;
            _environment = environment;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                var response = _environment.IsDevelopment() ?
                    new ResponseAPI((int)HttpStatusCode.InternalServerError, ex.Message, ex.StackTrace)
                    : new ResponseAPI((int)HttpStatusCode.InternalServerError, ex.Message);
                var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(jsonResponse);
            }
        }
        private void ApplySecurity(HttpContext context)
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";

            context.Response.Headers["X-Frame-Options"] = "DENY";

            // 3. نظام الأمان الأساسي الحديث (CSP) - عدله حسب احتياج مشروعك
            context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; object-src 'none';";

            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

        }
    }
}
