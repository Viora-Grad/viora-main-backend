using Viora.Application.Abstractions.Security;

namespace Viora.Infrastructure.Security;

internal class Hasher : IHasher
{
    public string Hash(string password)
    {
        var hashed = BCrypt.Net.BCrypt.HashPassword(password);
        return hashed;
    }

    public bool Verify(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}