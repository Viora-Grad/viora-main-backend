using Viora.Domain.Abstractions;
using Viora.Domain.Orders.Internal;

namespace Viora.Domain.Orders;

public abstract class Order : Entity
{
    public Guid OrganizationId { get; private set; }
    public double TotalPrice { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public Guid? SubscriptionId { get; private set; }
    public OrderStatus Status { get; private set; }

    protected Order()
    {
        // Required by EF Core
    }
    protected Order(Guid id, Guid organizationId, Guid? subscriptionId, double totalPrice, DateTime createdDate, OrderStatus status) : base(id)
    {
        OrganizationId = organizationId;
        SubscriptionId = subscriptionId;
        TotalPrice = totalPrice;
        CreatedDate = createdDate;
        Status = status;
    }
    protected Order(Guid id, Guid organizationId, double totalPrice, DateTime createdDate, OrderStatus status) : base(id)
    {
        OrganizationId = organizationId;
        TotalPrice = totalPrice;
        CreatedDate = createdDate;
        Status = status;
    }

}
