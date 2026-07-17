namespace Elattba.Application.Auth;

public interface IEmailSender
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
}
