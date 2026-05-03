using Viora.Domain.Abstractions;
using Viora.Domain.Users.Identity;

namespace Viora.Application.Abstractions.Authentication;

public interface IAuthenticationService
{
    Task<Result<string>> RegisterAsync(User user, string password, CancellationToken cancellationToken = default); // Returns the user ID of the newly registered user
    Task<Result<AuthResult>> LocalLoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<Result<AuthResult>> SocialLoginAsync(string provider, string providerKey, SocialInput input, CancellationToken cancellationToken = default);
    Task<Result<AuthResult>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

}

public sealed record AuthResult(Guid UserId,
    string AccessToken,
    string RefreshToken,
    ICollection<string> Roles,
    ICollection<string> Permissions);

// TODO: consider adding more properties to the AuthResult record, such as user roles, permissions(They do not exist yet),
// or other relevant information that might be needed for authorization purposes in the application.

