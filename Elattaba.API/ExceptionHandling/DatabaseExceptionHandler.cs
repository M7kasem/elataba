using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Elattaba.API.ExceptionHandling;

public sealed class DatabaseExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<DatabaseExceptionHandler> _logger;

    public DatabaseExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<DatabaseExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DbUpdateException)
        {
            return false;
        }

        _logger.LogError(exception, "A database update error occurred.");

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Title = "Database Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "A database error occurred while processing the request."
            }
        });
    }
}
