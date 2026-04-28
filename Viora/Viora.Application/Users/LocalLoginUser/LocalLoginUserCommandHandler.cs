using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;

namespace Viora.Application.Users.LocalLoginUser;

public sealed class LocalLoginUserCommandHandler(IAuthenticationService authenticationService) : ICommandHandler<LocalLoginUserCommand, AuthResult>
{
    public async Task<Result<AuthResult>> Handle(LocalLoginUserCommand request, CancellationToken cancellationToken)
    {

        var authResult = await authenticationService.LocalLoginAsync(request.Email, request.Password, cancellationToken);

        if (authResult.IsFailure)
            return Result.Failure<AuthResult>(authResult.Error);

        return Result.Success(authResult.Value);
    }

}
