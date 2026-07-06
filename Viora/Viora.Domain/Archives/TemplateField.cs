using Viora.Domain.Abstractions;
using Viora.Domain.Archives.Internals;

namespace Viora.Domain.Archives;

public class TemplateField : Entity
{
    public Guid TemplateVersionId { get; private set; }
    public TemplateName Name { get; private set; }
    public TemplateFieldLabel Label { get; private set; }
    public bool Required { get; private set; }
    public FieldType Type { get; private set; }
    public int Order { get; private set; }
    public FieldValidation Validation { get; private set; }
    public FieldLayout Layout { get; private set; }


    protected TemplateField() { }

    private TemplateField(
        Guid id,
        Guid templateVersionId,
        TemplateName name,
        TemplateFieldLabel label,
        FieldType fieldType,
        bool required,
        int order,
        FieldValidation validation,
        FieldLayout layout) : base(id)
    {
        TemplateVersionId = templateVersionId;
        Name = name;
        Label = label;
        Type = fieldType;
        Required = required;
        Order = order;
        Validation = validation;
        Layout = layout;
    }

    public static TemplateField Create(
        Guid templateVersionId,
        TemplateName name,
        TemplateFieldLabel label,
        FieldType fieldType,
        bool required,
        int order,
        FieldValidation validation,
        FieldLayout layout)
    {
        return new TemplateField(
            Guid.NewGuid(),
            templateVersionId,
            name,
            label,
            fieldType,
            required,
            order,
            validation,
            layout);
    }
}
