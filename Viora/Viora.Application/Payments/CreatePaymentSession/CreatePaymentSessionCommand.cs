using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Payments.CreatePaymentSession;

// Starts payment for a Draft order: issues an invoice, marks the order Pending,
// and creates a hosted Kashier session. Returns the hosted-pay URL.
public sealed record CreatePaymentSessionCommand(Guid OrderId) : ICommand<CreatePaymentSessionResponse>;

public sealed record CreatePaymentSessionResponse(string PaymentUrl);
