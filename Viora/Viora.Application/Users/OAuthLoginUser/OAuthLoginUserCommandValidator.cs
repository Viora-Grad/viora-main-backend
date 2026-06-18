using FluentValidation;

namespace Viora.Application.Users.OAuthLoginUser;

internal class OAuthLoginUserCommandValidator : AbstractValidator<OAuthLoginUserCommand>
{
    public OAuthLoginUserCommandValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty()
            .NotNull();
        RuleFor(x => x.Token)
            .NotEmpty()
            .When(x => string.IsNullOrEmpty(x.Code))
            .WithMessage("Token is required when Code is not provided.");
        RuleFor(x => x.Code)
            .NotEmpty()
            .When(x => string.IsNullOrEmpty(x.Token))
            .WithMessage("Code is required when Token is not provided.");
        RuleFor(x => x.RedirectUri)
            .NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.Code))
            .WithMessage("RedirectUri is required when Code is provided.");
        RuleFor(x => x)
            .Must(x => string.IsNullOrEmpty(x.Token) ^ string.IsNullOrEmpty(x.Code))
            .WithMessage("Either Token or Code must be provided and not both.");
    }
}
