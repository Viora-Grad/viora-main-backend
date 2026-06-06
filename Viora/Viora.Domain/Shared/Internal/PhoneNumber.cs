using System.Text.RegularExpressions;

namespace Viora.Domain.Shared.Internal;

public sealed partial record PhoneNumber
{
    private static readonly Regex _phoneRegex = MyRegex();

    public string Value { get; init; }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be empty.", nameof(value));


        if (!_phoneRegex.IsMatch(value))
            throw new ArgumentException($"The value '{value}' is not a valid phone number.", nameof(value));

        Value = value;
    }

    public static implicit operator PhoneNumber(string value) => new(value);
    public static implicit operator string(PhoneNumber? number) => number?.Value ?? string.Empty;

    [GeneratedRegex(@"^\+?[0-9\s\-\(\)]{7,15}$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex MyRegex();
}