namespace Viora.Domain.Plans.Internal;

public record PlanDescription(string Value)
{
    public static PlanDescription Create(string Value)
    {
        return new PlanDescription(Value);
    }
}
