namespace Viora.Application.Medias.Internals;

public record MimeType(string Value)
{
    public static implicit operator MimeType(string value) => new(value);
    public static implicit operator string(MimeType mimeType) => mimeType.Value;
};