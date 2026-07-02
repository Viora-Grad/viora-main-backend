namespace Viora.Domain.WalletTransactions.Internals;

public sealed record Description(string Value)
{
    public static implicit operator Description(string value) => new(value);
    public static implicit operator string(Description desctiption) => desctiption.Value;
}