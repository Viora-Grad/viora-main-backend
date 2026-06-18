using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Users.Identity;

namespace Viora.Application.Authentication.ValidateEmail;

internal class ValidateEmailCommandHandler(IUserRepository userRepository) : ICommandHandler<ValidateEmailCommand, string>
{
    public async Task<Result<string>> Handle(ValidateEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        var result = user != null ? "Email Exists for a User" : "Email Does Not Exist";
        return Result.Success(result);
    }
}
