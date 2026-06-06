namespace Viora.Domain.Inventory.Internals;

public sealed record Quantity(int Value)
{
    public static implicit operator Quantity(int value) => new(value);
    public static implicit operator int(Quantity item) => item.Value;
}