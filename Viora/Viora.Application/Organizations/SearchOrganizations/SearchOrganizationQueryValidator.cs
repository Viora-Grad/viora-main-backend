using FluentValidation;
using Viora.Domain.Organizations.Shared.Enums;

namespace Viora.Application.Organizations.SearchOrganizations;

internal class SearchOrganizationQueryValidator : AbstractValidator<SearchOrganizationsQuery>
{
    public SearchOrganizationQueryValidator()
    {
        RuleFor(x => x.ServiceType)
            .Must(s => Enum.TryParse<ServiceType>(s, ignoreCase: true, out _))
            .WithMessage("Invalid service type.")
            .When(x => x.ServiceType != null);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than zero.");
    }
}
