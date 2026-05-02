namespace Viora.Domain.Users.Internal;

public sealed record Email
{
    public string Value { get; init; }
    public Email(string value)
    {
        Value = value.ToLowerInvariant().Trim();
    }


}