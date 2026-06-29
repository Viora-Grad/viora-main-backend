using Viora.Domain.Abstractions;
using Viora.Domain.Orders.Internal;
using Viora.Domain.Shared;

namespace Viora.Domain.Orders;

public abstract class Order : Entity
{
    public Guid OrganizationId { get; private set; }
    public Guid? InvoiceId { get; private set; }
    public Money TotalPrice { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public Guid? SubscriptionId { get; private set; }
    public OrderStatus Status { get; private set; }

    protected Order()
    {
        // Required by EF Core
    }
    protected Order(Guid id, Guid organizationId, Guid? subscriptionId, Money totalPrice, DateTime createdDate, OrderStatus status) : base(id)
    {
        OrganizationId = organizationId;
        SubscriptionId = subscriptionId;
        TotalPrice = totalPrice;
        CreatedDate = createdDate;
        Status = status;
    }
    protected Order(Guid id, Guid organizationId, Money totalPrice, DateTime createdDate, OrderStatus status) : base(id)
    {
        OrganizationId = organizationId;
        TotalPrice = totalPrice;
        CreatedDate = createdDate;
        Status = status;
    }

    // Links a freshly-issued invoice to this Draft order.
    public Result AttachInvoice(Guid invoiceId)
    {
        if (InvoiceId is not null)
            return Result.Failure(OrderError.InvoiceAlreadyAttached);

        if (Status != OrderStatus.Draft)
            return Result.Failure(OrderError.InvalidStatusTransition);

        InvoiceId = invoiceId;
        return Result.Success();
    }

    // Draft -> Pending, set when a payment session is created.
    public Result MarkPending()
    {
        if (Status != OrderStatus.Draft)
            return Result.Failure(OrderError.InvalidStatusTransition);

        Status = OrderStatus.Pending;
        return Result.Success();
    }

    // Pending -> Paid. "Already paid" is distinguishable so webhooks treat replays as no-ops.
    public Result MarkPaid()
    {
        if (Status == OrderStatus.Paid || Status == OrderStatus.Fullfiled)
            return Result.Failure(OrderError.AlreadyPaid);

        if (Status != OrderStatus.Pending)
            return Result.Failure(OrderError.InvalidStatusTransition);

        Status = OrderStatus.Paid;
        return Result.Success();
    }

    // Pending -> Failed, set when the gateway reports a failed payment.
    public Result MarkFailed()
    {
        if (Status != OrderStatus.Pending)
            return Result.Failure(OrderError.InvalidStatusTransition);

        Status = OrderStatus.Failed;
        return Result.Success();
    }
}
