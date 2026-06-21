using FluentValidation;
using Viora.Domain.Users.Internal;

namespace Viora.Application.Users.OAuthRegisterUser;

internal class OAuthRegisterUserCommandValidator : AbstractValidator<OAuthRegisterUserCommand>
{
    public OAuthRegisterUserCommandValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty()
            .NotNull();
        RuleFor(x => x.Email)
            .NotEmpty()
            .NotNull()
            .EmailAddress();
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .NotNull();
        RuleFor(x => x.LastName)
            .NotEmpty()
            .NotNull();
        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .NotNull()
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date of birth must be in the past.");
        RuleFor(x => x.Gender)
            .NotEmpty()
            .NotNull()
            .Must(g => Enum.TryParse<Gender>(g, true, out _))
            .WithMessage("Invalid gender.");
    }
}
