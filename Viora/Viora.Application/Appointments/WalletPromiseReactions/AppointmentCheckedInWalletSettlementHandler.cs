using MediatR;
using Microsoft.Extensions.Logging;
using Viora.Application.Wallets.CompletePromise;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Events;
using Viora.Domain.Appointments.Internal;
using Viora.Domain.WalletPromises;

namespace Viora.Application.Appointments.WalletPromiseReactions;

// On check-in, settle the wallet promise: the held funds move to the branch wallet and the scheduled
// expiry is cancelled. No-op for non-wallet appointments. Runs alongside the schedule-notifier handler.
internal sealed class AppointmentCheckedInWalletSettlementHandler(
    IAppointmentsRepository appointmentsRepository,
    IWalletPromiseRepository promiseRepository,
    ISender sender,
    ILogger<AppointmentCheckedInWalletSettlementHandler> logger) : INotificationHandler<AppointmentCheckedInEvent>
{
    public async Task Handle(AppointmentCheckedInEvent notification, CancellationToken cancellationToken)
    {
        var appointment = await appointmentsRepository.GetByIdAsync(notification.Id, cancellationToken);
        if (appointment is null || appointment.PayMethod != PaymentMethod.Wallet || appointment.PaymentId is null)
            return;

        var promise = await promiseRepository.GetBySourceTransactionIdAsync(appointment.PaymentId.Value, cancellationToken);
        if (promise is null)
        {
            logger.LogWarning("Check-in settlement: no promise for appointment {AppointmentId} (payment {PaymentId}).",
                appointment.Id, appointment.PaymentId);
            return;
        }

        var result = await sender.Send(new CompletePromiseCommand(promise.Id), cancellationToken);
        if (result.IsFailure)
            logger.LogError("Check-in settlement failed for promise {PromiseId}: {Error}.", promise.Id, result.Error.Name);
    }
}
