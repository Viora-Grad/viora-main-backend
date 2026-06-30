namespace Viora.Domain.Staffs.Internal;

public sealed record HashedPassword(string Value)
{
    public static implicit operator HashedPassword(string value) => new(value);
    public static implicit operator string(HashedPassword hashedPassword) => hashedPassword.Value;
}
