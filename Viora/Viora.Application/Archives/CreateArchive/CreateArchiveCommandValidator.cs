using FluentValidation;

namespace Viora.Application.Archives.CreateArchive;

internal class CreateArchiveCommandValidator : AbstractValidator<CreateArchiveCommand>
{
    public CreateArchiveCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}
