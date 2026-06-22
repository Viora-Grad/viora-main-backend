using FluentValidation;

namespace Viora.Application.LegalPapers.UpdateLegalPaperStatus;

internal class UpdateLegalPaperStatusCommandValidator : AbstractValidator<UpdateLegalPaperStatusCommand>
{
    public UpdateLegalPaperStatusCommandValidator()
    {
        RuleFor(x => x.LegalPaperId)
            .NotEmpty()
            .WithMessage("legal paper id can not be empty");

        RuleFor(x => x.AdminId)
            .NotEmpty()
            .WithMessage("Admin Id can not be empty");
    }
}
