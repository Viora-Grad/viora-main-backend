namespace Viora.Domain.Organizations.OnBoardings.Internals;

public record Name(string Value)
{
    public static implicit operator Name(string value) => new(value);
    public static implicit operator string(Name name) => name.Value;
}