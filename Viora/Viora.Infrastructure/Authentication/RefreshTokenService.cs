using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using Viora.Application.Abstractions.Clock;

namespace Viora.Infrastructure.Authentication;

public class RefreshTokenService(IConfiguration config, IDateTimeProvider timeProvider)
{
    readonly int ExpiresIn = config.GetValue<int>("RefreshToken:Expiry_Days");
    readonly string Secret = config.GetValue<string>("RefreshToken:Secret") ?? throw new InvalidOperationException("Configuration value 'RefreshToken:Secret' is missing. Ensure REFRESH_TOKEN__SECRET is set in the .env file or environment variables.");

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
        ArgumentNullException.ThrowIfNull(token);
        using var hmac = new HMACSHA256(System.Text.Encoding.UTF8.GetBytes(Secret));
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hash);
    }
    public DateTime GetExpiryDate()
    {
        return timeProvider.UtcNow.AddDays(ExpiresIn);
    }


}
