namespace Viora.Domain.Inventory.Internals;

public sealed record Notes(string Value)
{
    public static implicit operator Notes(string value) => new(value);
    public static implicit operator string(Notes item) => item.Value;
}