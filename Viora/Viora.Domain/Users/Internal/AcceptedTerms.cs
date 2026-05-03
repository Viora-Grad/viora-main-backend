namespace Viora.Domain.Users.Internal;

public sealed record AcceptedTerms(DateTime AcceptedAt, int Version)
{
    public static AcceptedTerms Empty => new(DateTime.MinValue, 0);
}

