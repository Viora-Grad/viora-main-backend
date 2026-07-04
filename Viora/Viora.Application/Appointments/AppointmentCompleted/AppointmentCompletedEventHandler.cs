using MediatR;
using Viora.Domain.Appointments.Events;

namespace Viora.Application.Appointments.AppointmentCompleted;

// Wallet settlement now happens on check-in (see AppointmentCheckedInWalletSettlementHandler), so
// completion has no wallet responsibility. Kept as a no-op hook for future completion-time side effects.
public class AppointmentCompletedEventHandler : INotificationHandler<AppointmentCompletedEvent>
{
    public Task Handle(AppointmentCompletedEvent notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
