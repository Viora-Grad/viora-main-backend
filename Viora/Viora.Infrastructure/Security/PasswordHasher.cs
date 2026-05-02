using Viora.Application.Abstractions.Security;

namespace Viora.Infrastructure.Security;

internal class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        var hashed = BCrypt.Net.BCrypt.HashPassword(password);
        return hashed;
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}