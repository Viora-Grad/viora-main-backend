using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;

namespace Viora.Application.Authentication.ConsumeRefreshToken;

public class ConsumeRefreshTokenCommandHandler(IAuthenticationService authenticationService) : ICommandHandler<ConsumeRefreshTokenCommand, AuthResult>
{
    public async Task<Result<AuthResult>> Handle(ConsumeRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await authenticationService.RefreshTokenAsync(request.RefreshToken, cancellationToken);

    }
}
