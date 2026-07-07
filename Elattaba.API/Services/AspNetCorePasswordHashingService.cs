using ElAtaba.Domain.Entities;
using Elattba.Application.Users;
using Microsoft.AspNetCore.Identity;

namespace Elattaba.API.Services;

public sealed class AspNetCorePasswordHashingService : IPasswordHashingService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(User user, string hashedPassword, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, hashedPassword, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
