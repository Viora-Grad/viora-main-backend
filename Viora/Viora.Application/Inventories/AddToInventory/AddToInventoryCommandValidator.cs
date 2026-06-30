namespace Viora.Application.Inventories.AddToInventory;

using FluentValidation;

internal class AddToInventoryCommandValidator : AbstractValidator<AddToInventoryCommand>
{
    public AddToInventoryCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .NotEmpty().WithMessage("Branch ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Item name is required.")
            .MaximumLength(100).WithMessage("Item name cannot exceed 100 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.");

        RuleFor(x => x.MinimumThreshold)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum threshold cannot be negative.");
    }
}