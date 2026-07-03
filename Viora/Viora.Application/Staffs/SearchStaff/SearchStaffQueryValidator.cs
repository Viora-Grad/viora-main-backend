using FluentValidation;

namespace Viora.Application.Staffs.SearchStaff;

internal class SearchStaffQueryValidator : AbstractValidator<SearchStaffQuery>
{
    public SearchStaffQueryValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .When(x => x.LastName != null)
            .WithMessage("First name is required when last name is provided.");
    }
}
