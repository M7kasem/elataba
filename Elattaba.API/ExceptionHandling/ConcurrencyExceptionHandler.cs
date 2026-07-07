using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Elattaba.API.ExceptionHandling;

public sealed class ConcurrencyExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<ConcurrencyExceptionHandler> _logger;

    public ConcurrencyExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<ConcurrencyExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DbUpdateConcurrencyException)
        {
            return false;
        }

        _logger.LogWarning(exception, "A concurrency conflict occurred.");

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Title = "Concurrency Conflict",
                Status = StatusCodes.Status409Conflict,
                Detail = "The resource was modified by another request. Please reload and try again."
            }
        });
    }
}
