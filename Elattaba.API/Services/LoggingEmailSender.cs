using Elattba.Application.Auth;
using Microsoft.Extensions.Logging;

namespace Elattaba.API.Services;

public class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        _logger.LogInformation("Simulating email send to {Email} with reset link: {ResetLink}", toEmail, resetLink);
        return Task.CompletedTask;
    }
}
