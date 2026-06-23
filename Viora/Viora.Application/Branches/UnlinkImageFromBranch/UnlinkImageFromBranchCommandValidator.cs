using FluentValidation;

namespace Viora.Application.Branches.UnlinkImageFromBranch;

internal class UnlinkImageFromBranchCommandValidator : AbstractValidator<UnlinkImageFromBranchCommand>
{
    public UnlinkImageFromBranchCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .NotEmpty()
            .WithMessage("Branch Id can not be null");

        RuleFor(x => x.ImageId)
            .NotEmpty()
            .WithMessage("Image Id can not be null");
    }
}
