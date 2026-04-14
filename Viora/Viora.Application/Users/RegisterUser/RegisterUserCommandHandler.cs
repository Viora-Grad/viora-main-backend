using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Users;
using Viora.Domain.Users.Internal;

namespace Viora.Application.Users.RegisterUser;

public sealed class RegisterUserCommandHandler(IUserRepository userRepository,
    IAuthenticationService authenticationService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = User.Create(
        new FirstName(request.FirstName),
        new LastName(request.LastName),
        new UserName(request.UserName),
        new Email(request.Email),
        new Age(request.Age)
        );

        var registrationResult = await authenticationService.RegisterAsync(user, request.Password, cancellationToken);
        if (registrationResult.IsFailure)
        {
            return Result.Failure<Guid>(registrationResult.Error);
        }

        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}