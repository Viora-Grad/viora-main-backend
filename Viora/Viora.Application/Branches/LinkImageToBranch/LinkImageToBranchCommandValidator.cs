using FluentValidation;

namespace Viora.Application.Branches.LinkImageToBranch;

internal class LinkImageToBranchCommandValidator : AbstractValidator<LinkImageToBranchCommand>
{
    public LinkImageToBranchCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .NotEmpty()
            .WithMessage("Branch Id can not be null");

        RuleFor(x => x.MediaId)
            .NotEmpty()
            .WithMessage("Image Id can not be null");
    }

}
