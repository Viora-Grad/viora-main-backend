namespace Viora.Domain.Staffs.Internal;

public sealed record LastName(string Value)
{
    public static implicit operator LastName(string value) => new(value);
    public static implicit operator string(LastName lastName) => lastName.Value;
}
