namespace Viora.Domain.Users.Internal;

public sealed record Age(int Value)
{
    public static readonly Age Empty = new(0);
}
