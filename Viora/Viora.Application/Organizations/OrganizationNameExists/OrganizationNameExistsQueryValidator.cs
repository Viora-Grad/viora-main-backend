using FluentValidation;

namespace Viora.Application.Organizations.OrganizationNameExists;

internal sealed class OrganizationNameExistsQueryValidator : AbstractValidator<OrganizationNameExistsQuery>
{
    public OrganizationNameExistsQueryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name can not be empty or null")
            .MaximumLength(50)
            .WithMessage("Maximum allowed length is 50");
    }
}
