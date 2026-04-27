using Viora.Domain.Abstractions;

namespace Viora.Domain.Orders;

public class OrderError
{
    public static readonly Error InvalidSubscriptionOrderType =
        new Error("Invalid subscription order type", "The provided subscription order type is invalid.", ErrorCategory.Validation);
}
