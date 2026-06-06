using FluentValidation;

namespace Viora.Application.Organizations.RequestOnboard;

public class RequestOnboardCommandValidator : AbstractValidator<RequestOnboardCommand>
{
    public RequestOnboardCommandValidator()
    {
        RuleFor(x => x.OwnerId)
            .NotEmpty()
            .NotNull();

        RuleFor(x => x.CountryId)
            .NotEmpty()
            .NotNull();

        RuleFor(x => x.ProposedName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.About)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.ServiceDescription)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.ServiceTypes)
            .NotEmpty().WithMessage("At least one service type is required.");

        RuleForEach(x => x.ServiceTypes)
            .IsInEnum();

        RuleFor(x => x.ReferralSource)
            .IsInEnum();

        RuleFor(x => x.BillingEmail)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.SupportEmail)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Letter)
            .NotEmpty()
            .MaximumLength(1000);
    }
}
