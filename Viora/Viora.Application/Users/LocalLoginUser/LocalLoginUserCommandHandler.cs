using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;

namespace Viora.Application.Users.LocalLoginUser;

public sealed class LocalLoginUserCommandHandler(
    IAuthenticationService authenticationService,
    IJwtService jwtService,
    IUnitOfWork unitOfWork) : ICommandHandler<LocalLoginUserCommand>
{
    public async Task<Result> Handle(LocalLoginUserCommand request, CancellationToken cancellationToken)
    {


    }

}
