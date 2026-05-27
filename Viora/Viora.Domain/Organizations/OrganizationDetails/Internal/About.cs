namespace Viora.Domain.Organizations.OrganizationDetails.Internal;

public record About(string Value)
{
    public static implicit operator About(string value) => new(value);
    public static implicit operator string(About about) => about.Value;
}
