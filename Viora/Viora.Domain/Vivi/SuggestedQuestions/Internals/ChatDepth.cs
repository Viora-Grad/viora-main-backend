namespace Viora.Domain.Vivi.SuggestedQuestions.Internals;

public sealed record class ChatDepth(int Value)
{
    public static implicit operator int(ChatDepth depth) => depth.Value;
    public static implicit operator ChatDepth(int value) => new(value);
}
