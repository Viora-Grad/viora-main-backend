namespace Viora.Domain.Plans.Internal;

public record PlanContent(string Value)
{
    public static PlanContent Create(string Value)
    {
        return new PlanContent(Value);
    }
}
