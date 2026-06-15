namespace Viora.Domain.Shared.Internal;

public sealed record Email(string Value)
{
    public static implicit operator Email(string Value) => new(Value);
    public static implicit operator string(Email Email) => Email.Value;
}
