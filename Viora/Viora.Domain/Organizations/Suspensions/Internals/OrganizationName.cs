namespace Viora.Domain.Organizations.Suspensions.Internals;

public record class OrganizationName(string Value)
{
    public static implicit operator OrganizationName(string Value) => new(Value);
    public static implicit operator string(OrganizationName Value) => Value.Value;
}