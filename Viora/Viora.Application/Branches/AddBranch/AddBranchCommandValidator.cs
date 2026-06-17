using FluentValidation;
using Viora.Domain.Shared;

namespace Viora.Application.Branches.AddBranch;

internal sealed class AddBranchCommandValidator : AbstractValidator<AddBranchCommand>
{
    public AddBranchCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();

        RuleFor(x => x.AddressNumber).GreaterThan(0);
        RuleFor(x => x.AddressStreet).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AddressCity).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AddressState).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AddressCountryId).NotEmpty();
        RuleFor(x => x.AddressPostalCode).GreaterThan(0);

        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);

        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress().MaximumLength(200);

        RuleFor(x => x.ServicesProvided)
            .NotEmpty().WithMessage("At least one service type is required.")
            .Must(s => s.All(t => ServiceType.All.Any(st => st.Value.Equals(t, StringComparison.OrdinalIgnoreCase))))
            .WithMessage("One or more service types are invalid.");

        RuleFor(x => x.TimeZoneId)
            .NotEmpty()
            .MaximumLength(100)
            .Must(tz => TimeZoneInfo.TryFindSystemTimeZoneById(tz, out _))
            .WithMessage("Invalid timezone identifier.");
    }
}
