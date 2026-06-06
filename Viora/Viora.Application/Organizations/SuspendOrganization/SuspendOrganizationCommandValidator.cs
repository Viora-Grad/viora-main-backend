using FluentValidation;
using Viora.Domain.Organizations.Suspensions.Internals;

namespace Viora.Application.Organizations.SuspendOrganization;

internal class SuspendOrganizationCommandValidator : AbstractValidator<SuspendOrganizationCommand>
{
    public SuspendOrganizationCommandValidator()
    {

        RuleFor(s => s.Reason)
            .Must(s => Enum.TryParse<SuspensionReason>(s, ignoreCase: true, out _))
            .WithMessage("Invalid Reason type.");

        RuleFor(s => s.Notes)
            .NotEmpty().WithMessage("Notes are required.")
            .MaximumLength(512).WithMessage("Notes cannot exceed 512 characters.");
    }
}