namespace Viora.Domain.Feedbacks.Internals;

public record Comment(string Value)
{
    public static implicit operator Comment(string value) => new(value);
    public static implicit operator string(Comment comment) => comment.Value;
}
