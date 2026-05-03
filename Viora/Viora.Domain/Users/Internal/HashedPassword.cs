namespace Viora.Domain.Users.Internal;

public sealed record HashedPassword(string Value)
{
    public static HashedPassword Empty => new(string.Empty);
}
