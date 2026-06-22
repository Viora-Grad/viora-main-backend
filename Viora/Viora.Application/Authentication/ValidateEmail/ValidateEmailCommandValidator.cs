using FluentValidation;

namespace Viora.Application.Authentication.ValidateEmail;

internal class ValidateEmailCommandValidator : AbstractValidator<ValidateEmailCommand>
{
    public ValidateEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
