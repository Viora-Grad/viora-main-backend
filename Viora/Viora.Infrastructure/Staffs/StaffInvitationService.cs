using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Staffs.Abstractions;

namespace Viora.Infrastructure.Staffs;

internal class StaffInvitationService(IConfiguration config, IDateTimeProvider timeProvider) : IStaffInvitationService
{
    private readonly string Secret = config["STAFF_TOKEN_SECRET_KEY"];
    private readonly int ExpirationDays = int.Parse(config["STAFF_TOKEN_EXPIRATION_DAYS"] ?? "2");
    public string GenerateInvitationToken()
    {
        var randomBytes = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
    public string HashInvitationToken(string token)
    {
        using var hmac = new HMACSHA256(System.Text.Encoding.UTF8.GetBytes(Secret));
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hash);
    }
    public DateTime GetExpiryDate()
    {
        return timeProvider.UtcNow.AddDays(ExpirationDays);
    }
}
