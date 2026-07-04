using FluentValidation;

namespace Viora.Application.Archives.CreateTemplateVersion;

internal class CreateTemplateVersionCommandValidator : AbstractValidator<CreateTemplateVersionCommand>
{
    public CreateTemplateVersionCommandValidator()
    {
        RuleFor(x => x.TemplateId).NotEmpty();
        RuleFor(x => x.Fields).NotEmpty().WithMessage("At least one field is required");
        RuleForEach(x => x.Fields).ChildRules(field =>
        {
            field.RuleFor(f => f.Name).NotEmpty().WithMessage("Field name is required");
            field.RuleFor(f => f.Label).NotEmpty().WithMessage("Field label is required");
            field.RuleFor(f => f.Order).GreaterThanOrEqualTo(0);
        });
    }
}
