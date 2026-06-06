namespace Viora.Domain.Organizations.Shared;

public record SupportEmail(string Value)
{
    public static implicit operator SupportEmail(string value) => new(value);
    public static implicit operator string(SupportEmail supportEmail) => supportEmail.Value;
}