using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;

namespace Viora.Application.Users.OAuthLoginUser;

public sealed class OAuthLoginUserCommandHandler(IAuthenticationService authenticationService) : ICommandHandler<OAuthLoginUserCommand, AuthResult>
{
    public async Task<Result<AuthResult>> Handle(OAuthLoginUserCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
