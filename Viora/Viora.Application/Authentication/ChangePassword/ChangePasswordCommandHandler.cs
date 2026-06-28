using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Mail;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Users.Identity;

namespace Viora.Application.Authentication.ChangePassword;

internal sealed class ChangePasswordCommandHandler(
    IAuthenticationService authService,
    IUserContext userContext,
    IUserRepository userRepository,
    IEmailSender emailSender,
    IDateTimeProvider dateTime) : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var result = await authService.ChangePassword(
            userContext.UserId,
            request.CurrentPassword,
            request.NewPassword,
            cancellationToken);

        if (result.IsFailure)
            return result;

        var user = await userRepository.GetByIdAsync(userContext.UserId, cancellationToken);
        if (user is not null)
            await emailSender.SendAsync(
                user.Email.Value,
                EmailTemplateFactory.PasswordChanged(request.Ip, dateTime.UtcNow),
                cancellationToken);

        return Result.Success();
    }
}
