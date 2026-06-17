namespace Viora.Domain.Branches.Internals;

public sealed record TimeZoneId(string Value)
{
    public static implicit operator TimeZoneId(string value) => new(value);
    public static implicit operator string(TimeZoneId value) => value.Value;
}
