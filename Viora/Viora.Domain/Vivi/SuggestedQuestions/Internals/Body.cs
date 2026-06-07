namespace Viora.Domain.Vivi.SuggestedQuestions.Internals;

public sealed record Body(string Value)
{
    public static implicit operator string(Body body) => body.Value;
    public static implicit operator Body(string value) => new(value);
}
