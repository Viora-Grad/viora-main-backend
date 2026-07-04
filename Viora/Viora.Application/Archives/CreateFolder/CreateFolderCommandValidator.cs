using FluentValidation;
using Viora.Domain.Archives.Internals;

namespace Viora.Application.Archives.CreateFolder;

internal class CreateFolderCommandValidator : AbstractValidator<CreateFolderCommand>
{
    public CreateFolderCommandValidator()
    {
        RuleFor(x => x.ArchiveId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => t == FolderType.Root.Value || t == FolderType.System.Value || t == FolderType.Normal.Value)
            .WithMessage("Folder type must be Root, System, or Normal");
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
    }
}
