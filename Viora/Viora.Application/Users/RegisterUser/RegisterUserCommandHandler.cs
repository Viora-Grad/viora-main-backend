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
        var existingUser = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
            return Result.Failure<Guid>(UserErrors.EmailInUse);

        var user = User.Create(
            new FirstName(request.FirstName),
            new LastName(request.LastName),
            new Email(request.Email),
            new Age(request.Age)
        );

        var registrationResult = await authenticationService.RegisterAsync(request.Email, request.Password, cancellationToken);
        if (string.IsNullOrEmpty(registrationResult))
        { }

        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}