using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Caching;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Mail;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;

namespace Viora.Application.Authentication.ConfirmForgetPassword;

public class ConfirmForgetPasswordCommandHandler(IAuthenticationService authService, ICacheService cacheService, IEmailSender emailSender, IDateTimeProvider dateTime) : ICommandHandler<ConfirmForgetPasswordCommand>
{
    public async Task<Result> Handle(ConfirmForgetPasswordCommand request, CancellationToken cancellationToken)
    {
        var otp = await cacheService.GetAsync<string>($"forget-password-{request.Email}", cancellationToken)
            ?? throw new NotFoundException("No reset requests were submitted on this email");

        if (otp != request.Otp)
            throw new ConflictException("Otp is not matching");

        var result = await authService.UpdatePassword(request.Email, request.NewPassword, cancellationToken);

        if (result.IsFailure)
            return Result.Failure(result.Error);

        await emailSender.SendAsync(request.Email, EmailTemplateFactory.PasswordChanged(request.Ip, dateTime.UtcNow), cancellationToken);

        return Result.Success();
    }
}
