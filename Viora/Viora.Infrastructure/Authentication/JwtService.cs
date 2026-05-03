using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;

namespace Viora.Infrastructure.Authentication;

internal class JwtService(IConfiguration configuration, IDateTimeProvider dateTimeProvider) : IJwtService
{
    public string GenerateToken(Guid userId, IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var subjectClaims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // unique identifier for the token
        };

        // Add additional claims
        if (claims != null)
        {
            subjectClaims.AddRange(claims);
        }

        // Create the JWT token
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: subjectClaims,
            notBefore: dateTimeProvider.UtcNow,
            expires: dateTimeProvider.UtcNow.AddMinutes(int.Parse(configuration["Jwt:Expiration_Minutes"])),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}