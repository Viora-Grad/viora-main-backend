using MediatR;
using Microsoft.Extensions.Logging;
using Viora.Application.Wallets.RefundPromise;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Internal;
using Viora.Domain.WalletPromises;

namespace Viora.Application.Appointments.WalletPromiseReactions;

// Shared glue for the cancel/no-show refund handlers: resolve the promise behind a wallet appointment
// and refund it. Idempotent downstream (RefundPromise no-ops if the promise is already resolved).
internal static class WalletPromiseReaction
{
    public static async Task RefundAsync(
        IAppointmentsRepository appointmentsRepository,
        IWalletPromiseRepository promiseRepository,
        ISender sender,
        ILogger logger,
        Guid appointmentId,
        CancellationToken cancellationToken)
    {
        var appointment = await appointmentsRepository.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment is null || appointment.PayMethod != PaymentMethod.Wallet || appointment.PaymentId is null)
            return;

        var promise = await promiseRepository.GetBySourceTransactionIdAsync(appointment.PaymentId.Value, cancellationToken);
        if (promise is null)
            return;

        var result = await sender.Send(new RefundPromiseCommand(promise.Id), cancellationToken);
        if (result.IsFailure)
            logger.LogError("Wallet refund failed for promise {PromiseId}: {Error}.", promise.Id, result.Error.Name);
    }
}
