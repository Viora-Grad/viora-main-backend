using MediatR;
using Microsoft.Extensions.Logging;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Events;
using Viora.Domain.WalletPromises;

namespace Viora.Application.Appointments.WalletPromiseReactions;

// Refund the held funds when a wallet appointment is cancelled. Runs alongside the slot-freed handler.
internal sealed class AppointmentCanceledWalletRefundHandler(
    IAppointmentsRepository appointmentsRepository,
    IWalletPromiseRepository promiseRepository,
    ISender sender,
    ILogger<AppointmentCanceledWalletRefundHandler> logger) : INotificationHandler<AppointmentCanceledEvent>
{
    public Task Handle(AppointmentCanceledEvent notification, CancellationToken cancellationToken)
        => WalletPromiseReaction.RefundAsync(appointmentsRepository, promiseRepository, sender, logger, notification.Id, cancellationToken);
}
