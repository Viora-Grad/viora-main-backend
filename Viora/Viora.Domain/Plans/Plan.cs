using Viora.Domain.Abstractions;
using Viora.Domain.Plans.Internal;

namespace Viora.Domain.Plans;

internal class Plan : Entity
{
    public PlanCode Code { get; private set; }
    public PlanName Name { get; private set; }
    public PlanDescription Description { get; private set; }
    public int Version { get; private set; }
    public PlanContent Content { get; private set; }
    public ContentForm ContentForm { get; private set; }

    private Plan(Guid Id, PlanCode code, PlanName name, PlanDescription description, int version, PlanContent content, ContentForm contentForm) : base(Id)
    {
        this.Code = code;
        this.Name = name;
        this.Description = description;
        this.Version = version;
        this.Content = content;
        this.ContentForm = contentForm;
    }

}
