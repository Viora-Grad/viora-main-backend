using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Shared;

namespace Viora.Application.Wallets.PromisePayment;

// Holds funds from a customer wallet for an appointment (escrow). Returns the source (hold) transaction
// id, which the appointment stores as its PaymentId. Invoked by the create-appointment flow.
public sealed record PromisePaymentCommand(
    Guid UserId,
    Guid BranchId,
    Money Amount,
    DateTime ReservationDate) : ICommand<Guid>;
