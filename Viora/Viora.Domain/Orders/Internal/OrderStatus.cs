namespace Viora.Domain.Orders.Internal;

public record OrderStatus(int id, string Value)
{
    public static readonly OrderStatus Pending = new OrderStatus(1, "Pending");
    public static readonly OrderStatus Completed = new OrderStatus(2, "Completed");
    public static readonly OrderStatus Failed = new OrderStatus(3, "Failed");
    public static readonly OrderStatus Draft = new OrderStatus(4, "Draft");

    public static OrderStatus FromId(int id)
    {
        return id switch
        {
            1 => Pending,
            2 => Completed,
            3 => Failed,
            4 => Draft,
            _ => throw new ArgumentException($"Invalid OrderStatus id: {id}")
        };

    }
}
