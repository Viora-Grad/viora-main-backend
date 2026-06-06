namespace Viora.Domain.Organizations.OnBoardings.Internals;

public record About(string Value)
{
    public static implicit operator About(string value) => new(value);
    public static implicit operator string(About about) => about.Value;
}
