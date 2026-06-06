namespace Viora.Domain.Organizations.Suspensions.Internals;

public record SuspensionNote(string Value)
{
    public static implicit operator SuspensionNote(string value) => new(value);
    public static implicit operator string(SuspensionNote Noted) => Noted.Value;
}