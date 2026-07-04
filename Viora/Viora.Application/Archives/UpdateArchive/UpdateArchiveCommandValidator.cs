using FluentValidation;

namespace Viora.Application.Archives.UpdateArchive;

internal class UpdateArchiveCommandValidator : AbstractValidator<UpdateArchiveCommand>
{
    public UpdateArchiveCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}
