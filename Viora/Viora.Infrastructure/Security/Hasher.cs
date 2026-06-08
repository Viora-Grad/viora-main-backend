using Viora.Application.Abstractions.Security;

namespace Viora.Infrastructure.Security;

internal class Hasher : IHasher
{
    public string Hash(string input)
    {
        var hashed = BCrypt.Net.BCrypt.HashPassword(input);
        return hashed;
    }

    public bool Verify(string provided, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(provided, hash);
    }
}