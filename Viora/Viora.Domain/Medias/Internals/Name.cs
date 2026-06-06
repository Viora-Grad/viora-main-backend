namespace Viora.Domain.Medias.Internals;

public record Name(string Value)
{
    public static implicit operator Name(string value) => new(value);
    public static implicit operator string(Name mediaKey) => mediaKey.Value;
}
