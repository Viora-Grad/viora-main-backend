using MediatR;
using Microsoft.Extensions.Logging;
using Viora.Domain.WalletPromises.Events;

namespace Viora.Application.Wallets.RefundPromise;

// Fired by the outbox when a promise's grace window elapses. Refunds the customer if the promise is
// still pending (RefundPromise is a no-op if it was already settled on check-in).
internal sealed class PaymentPromisedEventHandler(
    ISender sender,
    ILogger<PaymentPromisedEventHandler> logger) : INotificationHandler<PaymentPromisedEvent>
{
    public async Task Handle(PaymentPromisedEvent notification, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RefundPromiseCommand(notification.PaymentPromiseId), cancellationToken);
        if (result.IsFailure)
            logger.LogError("Promise expiry refund failed for {PromiseId}: {Error}.", notification.PaymentPromiseId, result.Error.Name);
    }
}
