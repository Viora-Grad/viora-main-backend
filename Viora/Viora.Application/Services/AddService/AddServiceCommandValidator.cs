using FluentValidation;
using Viora.Domain.Shared;

namespace Viora.Application.Services.AddService;

internal sealed class AddServiceCommandValidator : AbstractValidator<AddServiceCommand>
{
    public AddServiceCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Service name is required.")
            .MaximumLength(100).WithMessage("Service name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Service description is required.")
            .MaximumLength(500).WithMessage("Service description cannot exceed 500 characters.");

        RuleFor(x => x.ServiceType)
            .NotEmpty().WithMessage("Service type is required.")
            .Must(type => ServiceType.All.Any(st => st.Value.Equals(type, StringComparison.OrdinalIgnoreCase)))
            .WithMessage("The provided service type is not a recognized specialty.");

        RuleFor(x => x.Duration)
            .GreaterThan(TimeSpan.Zero).WithMessage("Duration must be greater than zero.");

        RuleFor(x => x.Cost).NotNull();
        RuleFor(x => x.Cost.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Cost cannot be negative.")
            .When(x => x.Cost is not null);
        RuleFor(x => x.Cost.Currency)
            .NotNull().WithMessage("A valid currency is required.")
            .When(x => x.Cost is not null);
    }
}
