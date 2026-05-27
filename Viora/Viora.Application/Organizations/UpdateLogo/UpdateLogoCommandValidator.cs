using FluentValidation;

namespace Viora.Application.Organizations.UpdateLogo;

internal class UpdateLogoCommandValidator : AbstractValidator<UpdateLogoCommand>
{
    public UpdateLogoCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.MediaId).NotEmpty();
    }
}
