using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Users;

namespace Viora.Application.Users.LocalLoginUser;

public sealed class LocalLoginUserCommandHandler(
    IUserRepository userRepository,
    ICustomerRepository customerRepository,
    IOwnerRepository ownerRepository,
    IAuthenticationService authenticationService,
    IJwtService jwtService,
    IUnitOfWork unitOfWork) : ICommandHandler<LocalLoginUserCommand, AuthResult>
{
    public async Task<Result<AuthResult>> Handle(LocalLoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken) ?? throw new NotFoundException(request.Email);

        await authenticationService.LocalLoginAsync(request.Email, request.Password, cancellationToken);

    }

}
