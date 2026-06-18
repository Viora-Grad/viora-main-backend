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
    IHasher Hasher,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IIdentityRepository identityRepository,
    RefreshTokenService refreshTokenService,
    LocalCredentialRepository localCredentialRepository,
    RefreshTokenRepository refreshTokenRepository,
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

        var passwordVerificationResult = Hasher.Verify(password, localCredential.HashedPassword);
        if (!passwordVerificationResult)
        {
            localCredential.RecordFailedLogin();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<AuthResult>(UserErrors.InvalidCredentials);
        }

        localCredential.ResetFailedLoginAttempts();
        var permissionClaims = user.Roles.SelectMany(r => r.Permissions).Select(p => new Claim("permission", p.Name));

        var refreshTokenValue = refreshTokenService.GenerateRefreshToken();
        var hashedRefreshToken = refreshTokenService.HashToken(refreshTokenValue);
        var refreshTokenExpiry = refreshTokenService.GetExpiryDate();
        var refreshToken = RefreshToken.Create(user.Id, hashedRefreshToken, refreshTokenExpiry, dateTimeProvider.UtcNow);

        var activeToken = await refreshTokenRepository.GetActiveTokenByUserIdAsync(user.Id, cancellationToken);
        activeToken?.Revoke();



        var authResult = new AuthResult(
            UserId: user.Id,
            AccessToken: jwtService.GenerateToken(user.Id, permissionClaims),
            RefreshToken: refreshTokenValue,
            Roles: user.Roles.Select(r => r.Name).ToList(),
            Permissions: user.Roles.SelectMany(r => r.Permissions).Select(p => p.Name).Distinct().ToList()
        );
        user.RecordLogin(dateTimeProvider.UtcNow);
        refreshTokenRepository.Add(refreshToken);
        var userIdentity = user.Identities.FirstOrDefault(i => i.Provider == "local");
        userIdentity?.RecordLogin(dateTimeProvider.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(authResult);

    }

    public async Task<Result<AuthResult>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var refreshTokenHash = refreshTokenService.HashToken(refreshToken);
        var existingToken = await refreshTokenRepository.GetByTokenAsync(refreshTokenHash, cancellationToken);
        if (existingToken is not null)
        {
            var isValid = refreshTokenHash == existingToken.TokenHash;
            if (!isValid || existingToken.Expires < dateTimeProvider.UtcNow)
            {
                return Result.Failure<AuthResult>(AuthenticationErrors.InvalidToken);
            }
            existingToken.Revoke();
            var userId = existingToken.UserId;
            var newRefreshTokenValue = refreshTokenService.GenerateRefreshToken();
            var hashedNewRefreshToken = refreshTokenService.HashToken(newRefreshTokenValue);
            var expiry = refreshTokenService.GetExpiryDate();
            var newRefreshToken = RefreshToken.Create(userId, hashedNewRefreshToken, expiry, dateTimeProvider.UtcNow);

            var user = await userRepository.GetByIdAsync(userId, cancellationToken);
            if (user is null)
            {
                return Result.Failure<AuthResult>(UserErrors.NotFound);
            }
            var permissionClaims = user.Roles.SelectMany(r => r.Permissions).Select(p => new Claim("permission", p.Name));

            var authResult = new AuthResult(
                UserId: user.Id,
                AccessToken: jwtService.GenerateToken(user.Id, permissionClaims),
                RefreshToken: newRefreshTokenValue,
                Roles: user.Roles.Select(r => r.Name).ToList(),
                Permissions: user.Roles.SelectMany(r => r.Permissions).Select(p => p.Name).Distinct().ToList()
            );
            refreshTokenRepository.Add(newRefreshToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(authResult);

        }
        return Result.Failure<AuthResult>(AuthenticationErrors.InvalidToken);

    }

    public async Task<Result<string>> RegisterAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        var existingUser = await userRepository.GetByEmailAsync(user.Email.Value, cancellationToken);
        if (existingUser is not null)
            return Result.Failure<string>(UserErrors.EmailInUse);

        dbContext.Attach(Role.Registered);

        var hashedPassword = Hasher.Hash(password);

        var localCredential = new LocalCredential(user.Id, hashedPassword);
        localCredentialRepository.Add(localCredential);

        var identity = AuthIdentity.Create("local", user.Id, user.Email.Value, dateTimeProvider.UtcNow);
        user.LinkIdentity(identity);

        identityRepository.Add(identity);
        userRepository.Add(user);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(user.Id.ToString());

    }

    public async Task<Result<AuthResult>> SocialLoginAsync(User user, AuthIdentity identity, CancellationToken cancellationToken = default)
    {
        identity.RecordLogin(dateTimeProvider.UtcNow);
        var permissions = user.Roles.SelectMany(r => r.Permissions).Distinct().ToList();
        var permissionClaims = permissions.Select(p => new Claim("permission", p.Name));

        var refreshTokenValue = refreshTokenService.GenerateRefreshToken();
        var hashedRefreshToken = refreshTokenService.HashToken(refreshTokenValue);
        var refreshTokenExpiry = refreshTokenService.GetExpiryDate();
        var refreshToken = RefreshToken.Create(user.Id, hashedRefreshToken, refreshTokenExpiry, dateTimeProvider.UtcNow);

        var activeToken = await refreshTokenRepository.GetActiveTokenByUserIdAsync(user.Id, cancellationToken);
        activeToken?.Revoke();


        var authResult = new AuthResult(
            UserId: user.Id,
            AccessToken: jwtService.GenerateToken(user.Id, permissionClaims),
            RefreshToken: refreshTokenValue,
            Roles: user.Roles.Select(r => r.Name).ToList(),
            Permissions: [.. permissions.Select(p => p.Name).Distinct()]
        );

        identity.RecordLogin(dateTimeProvider.UtcNow);
        user.RecordLogin(dateTimeProvider.UtcNow);
        refreshTokenRepository.Add(refreshToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(authResult);

    }

    public Task<Result<AuthResult>> SocialRegisterAsync(User user, AuthIdentity identity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
