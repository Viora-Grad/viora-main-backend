using FluentValidation;

namespace Viora.Application.Inventories.InventoryItemAction;

internal class InventoryItemActionCommandValidator : AbstractValidator<InventoryItemActionCommand>
{
    public InventoryItemActionCommandValidator()
    {
        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Item ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.ActionType)
            .IsInEnum().WithMessage("Invalid inventory action type specified.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");
    }
}