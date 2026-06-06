namespace Viora.Domain.Inventory.Internals;

public sealed record MinimumThreshold(int Value)
{
    public static implicit operator MinimumThreshold(int value) => new(value);
    public static implicit operator int(MinimumThreshold item) => item.Value;
}