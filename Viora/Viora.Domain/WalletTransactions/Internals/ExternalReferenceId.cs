namespace Viora.Domain.WalletTransactions.Internals;

public sealed record ExternalReferenceId(string Value)
{
    public static implicit operator ExternalReferenceId(string value) => new(value);
    public static implicit operator string(ExternalReferenceId id) => id.Value;
}