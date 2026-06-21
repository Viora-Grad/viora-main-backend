using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Users.Identity;

namespace Viora.Application.Users.OAuthValidateToken;

internal class OAuthValidateTokenCommandHandler(
    ITokenValidator tokenValidator,
    IGoogleAuthenticator googleAuthenticator,
    IUserRepository userRepository) : ICommandHandler<OAuthValidateTokenCommand, SocialTokenValidationResult>
{
    public async Task<Result<SocialTokenValidationResult>> Handle(OAuthValidateTokenCommand command, CancellationToken cancellationToken)
    {
        if (command.IsToken)
        {
            var result = await tokenValidator.ValidateSocialTokenAsync(command.Provider, command.Token!, cancellationToken);
            result.Value.IsUserExists = await userRepository.ExistsByEmailAsync(result.Value.Email, cancellationToken);
            return result;
        }

        if (command.IsCode)
        {
            var IdToken = await googleAuthenticator.GetGoogleIdTokenAsync(command.Code!, command.RedirectUri!, cancellationToken);
            var result = await tokenValidator.ValidateSocialTokenAsync(command.Provider, IdToken, cancellationToken);
            result.Value.IsUserExists = await userRepository.ExistsByEmailAsync(result.Value.Email, cancellationToken);
            return result;
        }
        return Result.Failure<SocialTokenValidationResult>(AuthenticationErrors.InvalidToken);
    }
}
