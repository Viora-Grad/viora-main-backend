namespace Viora.Domain.Notifications.Internal;

public sealed record Title(string Value)
{
    public static implicit operator string(Title title) => title.Value;
    public static implicit operator Title(string value) => new(value);
}