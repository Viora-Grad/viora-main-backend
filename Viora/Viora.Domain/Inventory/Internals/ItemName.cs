namespace Viora.Domain.Inventory.Internals;

public sealed record ItemName(string Value)
{
    public static implicit operator ItemName(string value) => new(value);
    public static implicit operator string(ItemName item) => item.Value;
}
