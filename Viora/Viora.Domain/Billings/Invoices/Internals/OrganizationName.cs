namespace Viora.Domain.Billings.Invoices.Internals;

public sealed record OrganizationName(string Value)
{
    public static implicit operator OrganizationName(string value) => new(value);
    public static implicit operator string(OrganizationName name) => name.Value;
}

