namespace Viora.Domain.Vivi.ChatSessions.Internals;

public sealed record Name(string Value)
{
    public static implicit operator string(Name name) => name.Value;
    public static implicit operator Name(string value) => new(value);
}
