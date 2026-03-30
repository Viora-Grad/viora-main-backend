namespace Viora.Domain.Plans.Internal;

public record PlanCode(string Value)
{
    public static PlanCode Create(string Value)
    {
        return new PlanCode(Value);
    }
}

