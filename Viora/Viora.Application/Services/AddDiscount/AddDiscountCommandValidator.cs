using FluentValidation;

namespace Viora.Application.Services.AddDiscount;

internal sealed class AddDiscountCommandValidator : AbstractValidator<AddDiscountCommand>
{
    public AddDiscountCommandValidator()
    {
        RuleFor(x => x.ServiceId).NotEmpty();

        RuleFor(x => x.DiscountOutOf100)
            .InclusiveBetween(0, 100).WithMessage("Discount must be between 0 and 100.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("A discount reason is required.")
            .MaximumLength(500).WithMessage("Discount reason cannot exceed 500 characters.");

        RuleFor(x => x.Duration)
            .GreaterThan(TimeSpan.Zero).WithMessage("Discount duration must be greater than zero.");
    }
}
