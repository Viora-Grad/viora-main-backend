namespace Viora.Domain.Organizations.OnBoardings.Internals;

public record Letter(string Value)
{
    public static implicit operator string(Letter letter) => letter.Value;
    public static implicit operator Letter(string value) => new(value);
}
