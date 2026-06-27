using System.Security.Cryptography;
using Viora.Application.Abstractions.Caching;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Mail;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Users.Identity;

namespace Viora.Application.Authentication.ForgetPassword;

internal class ForgetPasswordCommandHandler(
    IUserRepository userRepository,
    ICacheService cacheService,
    IEmailSender emailSender) : ICommandHandler<ForgetPasswordCommand>
{
    public async Task<Result> Handle(ForgetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email) ?? throw new NotFoundException($"User email {request.Email} was not found");
        int otpNumber = RandomNumberGenerator.GetInt32(100000, 1000000);
        var otp = otpNumber.ToString();

        TimeSpan expiry = TimeSpan.FromMinutes(20);
        await cacheService.SetAsync($"forget-password-{request.Email}", otp, expiry, cancellationToken);

        var isSent = await emailSender.SendAsync(request.Email, EmailTemplateFactory.ForgetPassword(otp, 20), cancellationToken);

        if (!isSent)
            throw new BadRequestException($"Failed to send email to {request.Email}");

        return Result.Success();
    }
}
