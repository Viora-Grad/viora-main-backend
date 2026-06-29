using FluentValidation;
using Viora.Domain.Shared;

namespace Viora.Application.Organizations.UpdateOrganizationProfile;

public sealed class UpdateOrganizationProfileCommandValidator : AbstractValidator<UpdateOrganizationProfileCommand>
{
    public UpdateOrganizationProfileCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty();

        // Subdomain identifier, e.g. "nile-river": lowercase letters/digits in hyphen-separated
        // segments, no leading/trailing hyphen, and no spaces.
        RuleFor(x => x.SubDomain)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Subdomain must be lowercase letters, digits and hyphens with no spaces (e.g. 'nile-river').");

        RuleFor(x => x.SupportEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(x => x.BillingEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(x => x.ServiceDescription)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.About)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.ServicesProvided)
            .NotEmpty().WithMessage("At least one service type is required.");

        RuleForEach(x => x.ServicesProvided)
            .Must(s => ServiceType.All.Any(t => t.Value.Equals(s, StringComparison.OrdinalIgnoreCase)))
            .WithMessage("One or more service types are invalid.");
    }
}
