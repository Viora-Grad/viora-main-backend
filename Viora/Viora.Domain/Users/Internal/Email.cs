namespace Viora.Domain.Users.Internal;

public sealed record Email
{
    string Value { get; init; }
    public Email(string value)
    {
        Value = value.ToLowerInvariant().Trim();
    }


}