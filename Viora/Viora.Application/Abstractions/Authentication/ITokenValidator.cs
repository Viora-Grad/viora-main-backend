using Viora.Domain.Abstractions;

namespace Viora.Application.Abstractions.Authentication;

public sealed record SocialTokenValidationResult(
    string Provider,
    string ProviderKey,   // external subject id (e.g., "sub")
    string? Email,
    string? FirstName,
    string? LastName,
    bool EmailVerified);

public sealed record SocialInput(
    string? Email,
    string? FirstName,
    string? LastName);

public interface ITokenValidator
{
    Task<Result<SocialTokenValidationResult>> ValidateSocialTokenAsync(
        string provider,
        string token,
        CancellationToken cancellationToken = default);
}