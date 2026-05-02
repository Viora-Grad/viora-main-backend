using System.Security.Claims;

namespace Viora.Application.Abstractions.Authentication;

/// <summary>
/// Defines functionality for generating JSON Web Tokens (JWT) for authenticated users.
/// </summary>
/// <remarks>Implementations of this interface should ensure that generated tokens include all necessary claims
/// for authentication and authorization. The claims parameter allows for extensibility, such as including role or
/// permission information in the token payload.</remarks>
public interface IJwtService
{
    string GenerateToken(Guid userId, IEnumerable<Claim> claims);

}
