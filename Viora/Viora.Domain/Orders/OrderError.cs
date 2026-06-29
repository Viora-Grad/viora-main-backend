using Viora.Domain.Abstractions;

namespace Viora.Domain.Orders;

public class OrderError
{
    public static readonly Error InvalidSubscriptionOrderType =
        new Error("Invalid subscription order type", "The provided subscription order type is invalid.", ErrorCategory.Validation);

    public static readonly Error NoAddon = new Error("no Addon", "there are not addon added in the order", ErrorCategory.Validation);

    public static readonly Error InvalidStatusTransition =
        new Error("Order.InvalidStatusTransition", "The order cannot transition from its current status.", ErrorCategory.Conflict);

    public static readonly Error InvoiceAlreadyAttached =
        new Error("Order.InvoiceAlreadyAttached", "An invoice is already attached to this order.", ErrorCategory.Conflict);

    public static readonly Error AlreadyPaid =
        new Error("Order.AlreadyPaid", "The order has already been paid.", ErrorCategory.Conflict);
}
