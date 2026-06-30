namespace Viora.Domain.Staffs.Internal;

public sealed record FirstName(string Value)
{
    public static implicit operator FirstName(string value) => new(value);
    public static implicit operator string(FirstName firstName) => firstName.Value;
}
