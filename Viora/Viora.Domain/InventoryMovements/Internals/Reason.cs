namespace Viora.Domain.InventoryMovements.Internals;

public sealed record Reason(string Value)
{
    public static implicit operator Reason(string value) => new(value);
    public static implicit operator string(Reason Value) => Value.Value;
}
