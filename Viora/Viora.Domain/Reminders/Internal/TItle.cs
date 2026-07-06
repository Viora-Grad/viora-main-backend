namespace Viora.Domain.Reminders.Internal;

public sealed record TItle
{
    public static implicit operator string(TItle title) => title.Value;
    public static implicit operator TItle(string value) => new(value);

    public string Value { get; init; }
    public TItle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Title cannot be null or whitespace.", nameof(value));

        if (value.Length > 200)
            throw new ArgumentException("Title cannot exceed 200 characters.", nameof(value));


        value = value.Trim();
        Value = value;
    }
}
