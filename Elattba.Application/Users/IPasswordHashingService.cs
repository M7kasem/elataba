using ElAtaba.Domain.Entities;

namespace Elattba.Application.Users;

public interface IPasswordHashingService
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string hashedPassword, string password);
}
