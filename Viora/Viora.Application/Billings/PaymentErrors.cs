using Viora.Domain.Abstractions;

namespace Viora.Application.Billings;

public static class PaymentErrors
{
    public static readonly Error OrderNotFound =
        new("Payment.OrderNotFound", "No order was found for the supplied id.", ErrorCategory.NotFound);

    public static readonly Error OrderNotPayable =
        new("Payment.OrderNotPayable", "The order is not in a state that can start a payment.", ErrorCategory.Conflict);

    public static readonly Error AmountMismatch =
        new("Payment.AmountMismatch", "The computed invoice total does not match the order total.", ErrorCategory.Internal);

    public static readonly Error GatewayUnreachable =
        new("Payment.GatewayUnreachable", "The payment gateway could not be reached.", ErrorCategory.BadGateway);

    public static readonly Error GatewayError =
        new("Payment.GatewayError", "The payment gateway returned an error.", ErrorCategory.BadGateway);

    public static readonly Error GatewayTimeout =
        new("Payment.GatewayTimeout", "The payment gateway timed out.", ErrorCategory.Timeout);

    public static readonly Error InvalidResponse =
        new("Payment.InvalidResponse", "The payment gateway returned an unexpected response.", ErrorCategory.BadGateway);

    public static readonly Error InvalidSignature =
        new("Payment.InvalidSignature", "The webhook signature could not be verified.", ErrorCategory.Unauthorized);

    public static readonly Error PayoutFailed =
        new("Payment.PayoutFailed", "The payout could not be completed at the gateway.", ErrorCategory.BadGateway);
}
