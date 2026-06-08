using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Viora.Application.Abstractions.Authentication;
using Viora.Domain.Abstractions;

namespace Viora.Infrastructure.Authentication;

// Uses Google as the only provider for now 
internal class TokenValidator(IConfiguration config) : ITokenValidator
{
    private readonly string googleClientId = config["Google:ClientId"]!;

    public async Task<Result<SocialTokenValidationResult>> ValidateSocialTokenAsync(string provider, string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [googleClientId]
            };
            var payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);
            var result = new SocialTokenValidationResult
            {
                Provider = provider,
                ProviderKey = payload.Subject,
                Email = payload.Email,
                EmailVerified = payload.EmailVerified
            };

            return Result.Success(result);

        }
        catch (InvalidJwtException ex)
        {
            throw new Exception(ex.Message);
        }
    }
}
