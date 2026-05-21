namespace Viora.Domain.Vivi.ChatSessions.Internals;

public sealed record ModelUsed(string Value)
{
    public static implicit operator string(ModelUsed model) => model.Value;
    public static implicit operator ModelUsed(string value) => new(value);
}
