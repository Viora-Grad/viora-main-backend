namespace Viora.Domain.Orders.Internal;

public record OrderStatus(int Id, string Value)
{
    public static readonly OrderStatus Draft = new(1, "Draft");
    public static readonly OrderStatus Pending = new(2, "Pending");
    public static readonly OrderStatus Paid = new(3, "Paid");
    public static readonly OrderStatus Fullfiled = new(4, "Fullfiled");
    public static readonly OrderStatus Failed = new(5, "Failed");

    public static OrderStatus FromId(int id)
    {
        return id switch
        {
            1 => Draft,
            2 => Pending,
            3 => Paid,
            4 => Fullfiled,
            5 => Failed,
            _ => throw new ArgumentException($"Invalid OrderStatus id: {id}")
        };

    }
}
