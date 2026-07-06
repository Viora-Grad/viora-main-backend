using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Users.Identity;

namespace Viora.Application.Authentication.ValidateEmail;

public class ValidateEmailCommandHandler(IUserRepository userRepository) : ICommandHandler<ValidateEmailCommand>
{
    public async Task<Result> Handle(ValidateEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is not null)
        {
            throw new ConflictException("Email is already in use.");
        }
        return Result.Success();


    }
}
