using FluentValidation;

namespace Viora.Application.Authentication.ForgetPassword;

internal class ForgetPasswordCommandValidator : AbstractValidator<ForgetPasswordCommand>
{
    public ForgetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Must be email address")
            .NotEmpty()
            .WithMessage("Can not be empty email");
    }
}
