using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;

namespace Viora.Application.Users.OAuthValidateToken;

internal class OAuthValidateTokenCommandHandler(ITokenValidator tokenValidator) : ICommandHandler<OAuthValidateTokenCommand, SocialTokenValidationResult>
{
    public async Task<Result<SocialTokenValidationResult>> Handle(OAuthValidateTokenCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await tokenValidator.ValidateSocialTokenAsync(command.Provider, command.Token, cancellationToken);
        return validationResult.IsSuccess ?
            Result.Success(validationResult.Value) :
            Result.Failure<SocialTokenValidationResult>(validationResult.Error);
    }
}
