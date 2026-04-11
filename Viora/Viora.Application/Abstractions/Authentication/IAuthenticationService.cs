namespace Viora.Application.Abstractions.Authentication;

public interface IAuthenticationService
{
    Task<String> RegisterAsync(string email, string password, CancellationToken cancellationToken = default);

}
