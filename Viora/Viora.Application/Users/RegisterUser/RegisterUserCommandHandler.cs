using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Users.Identity;
using Viora.Domain.Users.Internal;

namespace Viora.Application.Users.RegisterUser;

public sealed class RegisterUserCommandHandler(
    IAuthenticationService authenticationService,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = User.Create(
        new PersonalInfo(request.FirstName, request.LastName, request.DateOfBirth, request.Gender),
        new Email(request.Email),
        dateTimeProvider.UtcNow
        );

        var registrationResult = await authenticationService.RegisterAsync(user, request.Password, cancellationToken);
        if (registrationResult.IsFailure)
        {
            return Result.Failure<Guid>(registrationResult.Error);
        }

        return Result.Success(user.Id);
    }
}