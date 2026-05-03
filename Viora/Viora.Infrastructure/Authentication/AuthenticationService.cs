using System.Security.Claims;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Security;
using Viora.Domain.Abstractions;
using Viora.Domain.Users.Identity;
using Viora.Infrastructure.Repositories.Authentication;

namespace Viora.Infrastructure.Authentication;

internal class AuthenticationService(IUserRepository userRepository,
    IJwtService jwtService,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    LocalCredentialRepository localCredentialRepository,
    ApplicationDbContext dbContext) : IAuthenticationService
{
    public async Task<Result<AuthResult>> LocalLoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null)
            return Result.Failure<AuthResult>(UserErrors.InvalidCredentials);



        var localCredential = await localCredentialRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (localCredential is null)
        {
            return Result.Failure<AuthResult>(UserErrors.InvalidCredentials); // User exists but doesn't have local credentials, could be a social login user
        }

        var passwordVerificationResult = passwordHasher.VerifyPassword(password, localCredential.HashedPassword);
        if (!passwordVerificationResult)
        {
            localCredential.RecordFailedLogin();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<AuthResult>(UserErrors.InvalidCredentials);
        }

        localCredential.ResetFailedLoginAttempts();

        var roleClaims = user.Roles.Select(r => new Claim(ClaimTypes.Role, r.Name));
        var permissionClaims = user.Roles.SelectMany(r => r.Permissions).Select(p => new Claim("permission", p.Name));
        var claims = roleClaims.Concat(permissionClaims).ToList();

        var authResult = new AuthResult(
            UserId: user.Id,
            AccessToken: jwtService.GenerateToken(user.Id, claims),
            RefreshToken: "placeholder_refresh_token",
            Roles: user.Roles.Select(r => r.Name).ToList(),
            Permissions: user.Roles.SelectMany(r => r.Permissions).Select(p => p.Name).Distinct().ToList()
        );
        user.RecordLogin(dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(authResult);

    }

    public Task<Result<AuthResult>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<string>> RegisterAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        var existingUser = await userRepository.GetByEmailAsync(user.Email.Value, cancellationToken);
        if (existingUser is not null)
            return Result.Failure<string>(UserErrors.EmailInUse);

        dbContext.Attach(Role.Registered);

        var hashedPassword = passwordHasher.HashPassword(password);

        var localCredential = new LocalCredential(user.Id, hashedPassword);
        localCredentialRepository.Add(localCredential);

        var identity = AuthIdentity.Create("local", user.Id, user.Email.Value, dateTimeProvider.UtcNow);
        user.LinkIdentity(identity);

        userRepository.Add(user);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(user.Id.ToString());

    }

    public Task<Result<AuthResult>> SocialLoginAsync(string provider,
        string providerKey,
        SocialInput input,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();

    }
}
