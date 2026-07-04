using FluentValidation;

namespace Viora.Application.Archives.CreateRecord;

internal class CreateRecordCommandValidator : AbstractValidator<CreateRecordCommand>
{
    public CreateRecordCommandValidator()
    {
        RuleFor(x => x.ArchiveId).NotEmpty();
        RuleFor(x => x.FolderId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.TemplateId).NotEmpty();
        RuleFor(x => x.TemplateVersion).GreaterThan(0).WithMessage("Template version must be greater than 0");
        RuleFor(x => x.Values).NotEmpty().WithMessage("At least one field value is required");
        RuleForEach(x => x.Values).ChildRules(value =>
        {
            value.RuleFor(v => v.FieldName).NotEmpty().WithMessage("Field name is required");
        });
    }
}
