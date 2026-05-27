using FluentValidation;
using Viora.Domain.Organizations.OnBoardings.Internals;
using Viora.Domain.Organizations.Shared.Enums;

namespace Viora.Application.Organizations.SearchApplications;

internal class SearchApplicationsQueryValidator : AbstractValidator<SearchApplicationsQuery>
{
    public SearchApplicationsQueryValidator()
    {
        RuleFor(x => x.ReferralSource)
            .Must(s => Enum.TryParse<ReferralSource>(s, ignoreCase: true, out _))
            .WithMessage("Invalid Referral Source type.")
            .When(x => x.ReferralSource != null);

        RuleFor(x => x.Status)
            .Must(s => Enum.TryParse<ApplicationStatus>(s, ignoreCase: true, out _))
            .WithMessage("Invalid Status Requested.")
            .When(x => x.Status != null);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than zero.");
    }
}
