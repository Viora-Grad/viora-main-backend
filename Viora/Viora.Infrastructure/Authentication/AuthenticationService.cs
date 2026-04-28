using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Security;
using Viora.Domain.Abstractions;
using Viora.Domain.Users.Customers;
using Viora.Domain.Users.Identity;
using Viora.Domain.Users.Owners;

namespace Viora.Infrastructure.Authentication;

internal class AuthenticationService(IUserRepository userRepository,
    ICustomerRepository customerRepository,
    IOwnerRepository ownerRepository,
    IJwtService jwtService,
    IPasswordHasher passwordHasher,
    ApplicationDbContext dbContext) : IAuthenticationService
{
    public async Task<Result<AuthResult>> LocalLoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();

    }

    public Task<Result<AuthResult>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<string>> RegisterAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<AuthResult>> SocialLoginAsync(string provider, string providerKey, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
