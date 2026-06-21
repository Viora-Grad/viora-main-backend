using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Users.Identity;

namespace Viora.Application.Users.OAuthLoginUser;

public sealed class OAuthLoginUserCommandHandler(
    IAuthenticationService authenticationService,
    IGoogleAuthenticator googleAuthenticator,
    ITokenValidator tokenValidator,
    IUserRepository userRepository,
    IIdentityRepository identityRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<OAuthLoginUserCommand, AuthResult>
{
    public async Task<Result<AuthResult>> Handle(OAuthLoginUserCommand request, CancellationToken cancellationToken)
    {
        // obtain provider key
        string idToken = request.Token ?? string.Empty;
        if (request.IsCode)
        {
            idToken = await googleAuthenticator.GetGoogleIdTokenAsync(request.Code!, request.RedirectUri!, cancellationToken);
        }

        var TokenValidationResult = await tokenValidator.ValidateSocialTokenAsync(request.Provider, idToken, cancellationToken);
        var providerKey = TokenValidationResult.Value.ProviderKey;
        var provider = TokenValidationResult.Value.Provider;
        var email = TokenValidationResult.Value.Email;

        var user = await userRepository.GetByEmailAsync(email, cancellationToken) ??
            throw new NotFoundException("User not found");

        var identity = await identityRepository.GetByProviderAsync(request.Provider, providerKey, cancellationToken);

        if (identity is null)
        {
            identity = AuthIdentity.Create(request.Provider, user.Id, providerKey, dateTimeProvider.UtcNow);
            var linkResult = user.LinkIdentity(identity);
            if (linkResult.IsFailure)
                return Result.Failure<AuthResult>(linkResult.Error);
            identityRepository.Add(identity);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        var authResult = await authenticationService.SocialLoginAsync(user, identity, cancellationToken);
        return authResult;



    }
}
