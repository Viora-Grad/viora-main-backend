namespace Viora.Domain.Billings.Invoices.Internals;

public sealed record Number(string Value)
{
    public static implicit operator Number(string value) => new(value);
    public static implicit operator string(Number number) => number.Value;
}
