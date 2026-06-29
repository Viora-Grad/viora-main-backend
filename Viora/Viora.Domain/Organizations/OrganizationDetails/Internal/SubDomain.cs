namespace Viora.Domain.Organizations.OrganizationDetails.Internal;

public sealed record SubDomain(string Value)
{
    public static implicit operator SubDomain(string value) => new(value);
    public static implicit operator string(SubDomain value) => value.Value;
}
