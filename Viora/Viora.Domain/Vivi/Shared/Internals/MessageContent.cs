namespace Viora.Domain.Vivi.Shared.Internals;

public record MessageContent(string Value)
{
    public static implicit operator string(MessageContent content) => content.Value;
    public static implicit operator MessageContent(string value) => new(value);
}
