using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;

namespace Viora.Application.Authentication.ConsumeStaffRefreshToken;

public class ConsumeStaffRefreshTokenCommandHandler(
    IAuthenticationService authenticationService
    ) : ICommandHandler<ConsumeStaffRefreshTokenCommand, AuthResult>
{
    public async Task<Result<AuthResult>> Handle(ConsumeStaffRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await authenticationService.RefreshStaffTokenAsync(request.RefreshToken, cancellationToken);
    }
}
