namespace Viora.Application.Medias.Internals;

public record MediaKey(string Value)
{
    public static implicit operator MediaKey(string value) => new(value);
    public static implicit operator string(MediaKey mediaKey) => mediaKey.Value;
}