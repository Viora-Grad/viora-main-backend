using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;

namespace Viora.Application.Users.LogoutUser;

internal class LogoutUserCommandHandler(
    IAuthenticationService authService
    ) : ICommandHandler<LogoutUserCommand>
{
    public async Task<Result> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        return await authService.LogoutAsync(request.RefreshToken, cancellationToken);
    }
}

