namespace Viora.Domain.Plans.Internal;

public record PlanName(string value)
{
    public static PlanName Create(string value)
    {
        return new PlanName(value);
    }
}