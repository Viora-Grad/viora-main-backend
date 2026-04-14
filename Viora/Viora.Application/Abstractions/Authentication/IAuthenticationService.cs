using Viora.Domain.Abstractions;
using Viora.Domain.Users;

namespace Viora.Application.Abstractions.Authentication;

public interface IAuthenticationService
{
    Task<Result<string>> RegisterAsync(User user, string password, CancellationToken cancellationToken = default); // Returns the user ID of the newly registered user

}
