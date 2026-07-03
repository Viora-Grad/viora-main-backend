using MediatR;
using Microsoft.Extensions.Logging;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Events;
using Viora.Domain.WalletPromises;

namespace Viora.Application.Appointments.WalletPromiseReactions;

// Refund the held funds when a wallet appointment is marked no-show.
internal sealed class AppointmentNoShowWalletRefundHandler(
    IAppointmentsRepository appointmentsRepository,
    IWalletPromiseRepository promiseRepository,
    ISender sender,
    ILogger<AppointmentNoShowWalletRefundHandler> logger) : INotificationHandler<AppointmentNoShowEvent>
{
    public Task Handle(AppointmentNoShowEvent notification, CancellationToken cancellationToken)
        => WalletPromiseReaction.RefundAsync(appointmentsRepository, promiseRepository, sender, logger, notification.AppointmentId, cancellationToken);
}
