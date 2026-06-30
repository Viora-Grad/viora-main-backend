using Viora.Domain.Abstractions;
using Viora.Domain.Medias;
using Viora.Domain.Prescriptions.Internals;

namespace Viora.Domain.Prescriptions;

public class PrescriptionTemplate : Entity
{
    public Guid OrganizationId { get; private set; }
    public TemplateName Name { get; private set; }
    public Guid? TemplateMediaId { get; private set; }
    public double TopMargin { get; private set; }
    public double RightMargin { get; private set; }
    public double LeftMargin { get; private set; }
    public double BottomMargin { get; private set; }

    public readonly MediaFile? File;
    protected PrescriptionTemplate() { }

    public PrescriptionTemplate(
        Guid id,
        Guid organizationId,
        string name,
        Guid? templateMediaId,
        double topMargin,
        double rightMargin,
        double leftMargin,
        double bottomMargin) : base(id)
    {
        OrganizationId = organizationId;
        Name = new TemplateName(name);
        TemplateMediaId = templateMediaId;
        TopMargin = topMargin;
        RightMargin = rightMargin;
        LeftMargin = leftMargin;
        BottomMargin = bottomMargin;
    }


    public static Result<PrescriptionTemplate> Create(
        Guid organizationId,
        string name,
        Guid? TemplateMediaId,
        double topMargin,
        double rightMargin,
        double leftMargin,
        double bottomMargin
        )
    {
        var Id = Guid.NewGuid();
        var template = new PrescriptionTemplate(
            Id,
            organizationId,
            name,
            TemplateMediaId,
            topMargin,
            rightMargin,
            leftMargin,
            bottomMargin);
        return Result.Success(template);
    }
}
