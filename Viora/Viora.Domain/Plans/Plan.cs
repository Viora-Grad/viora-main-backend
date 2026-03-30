using Viora.Domain.Abstractions;
using Viora.Domain.Plans.Internal;

namespace Viora.Domain.Plans;

public class Plan : Entity
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

    public static Result<Plan> Create(PlanCode code, PlanName name, PlanDescription description, int version, PlanContent content, string contentForm)
    {
        var contentFormResult = ContentForm.CheckContentForm(contentForm);
        if (!contentFormResult.IsSuccess)
        {
            return Result.Failure<Plan>(contentFormResult.Error);
        }
        var plan = new Plan(Guid.NewGuid(), code, name, description, version, content, contentFormResult.Value);
        return Result.Success(plan);
    }
}
