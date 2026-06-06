using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using Viora.Application.Abstractions.Clock;

namespace Viora.Infrastructure.Authentication;

internal class RefreshTokenService(IConfiguration config, IDateTimeProvider timeProvider)
{
    readonly int ExpiresIn = config.GetValue<int>("RefreshToken:ExpiryDays");
    readonly string Secret = config.GetValue<string>("RefreshToken:Secret")!;

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
    public string HashToken(string token)
    {
        using var hmac = new HMACSHA256(System.Text.Encoding.UTF8.GetBytes(Secret));
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hash);
    }
    public DateTime GetExpiryDate()
    {
        return timeProvider.UtcNow.AddDays(ExpiresIn);
    }


}
