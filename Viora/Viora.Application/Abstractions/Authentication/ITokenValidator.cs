using Viora.Domain.Abstractions;

namespace Viora.Application.Abstractions.Authentication;

public sealed record SocialTokenValidationResult
{
    public string Provider { get; set; } = null!;
    public string ProviderKey { get; set; } = null!;   // external subject id (e.g., "sub")
    public string Email { get; set; } = null!;
    public bool EmailVerified { get; set; }

}

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