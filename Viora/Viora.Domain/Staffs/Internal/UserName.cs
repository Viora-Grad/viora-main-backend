using System.Text.RegularExpressions;

namespace Viora.Domain.Staffs.Internal;

public sealed record Username
{
    private static readonly Regex _regex = new(@"^[a-zA-Z0-9_]{3,20}$", RegexOptions.Compiled);
    public string Value { get; }
    public Username(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Username cannot be null or whitespace.", nameof(value));
        if (!_regex.IsMatch(value))
            throw new ArgumentException("Username must be 3-20 characters long and can only contain letters, numbers, and underscores.", nameof(value));
        Value = value;
    }
    public static implicit operator Username(string value) => new(value);
    public static implicit operator string(Username username) => username.Value;

}
