using MediatR;
using Viora.Application.Abstractions.Notification;
using Viora.Domain.Appointments.Events;

namespace Viora.Application.Appointments.CancelAppointment;

public class AppointmentCanceledEventHandler(IScheduleNotifier scheduleNotifier) : INotificationHandler<AppointmentCanceledEvent>
{
    public async Task Handle(AppointmentCanceledEvent notification, CancellationToken cancellationToken)
    {
        await scheduleNotifier.NotifySlotFreedAsync(
            notification.BranchId,
            notification.Id,
            notification.ReservationDate,
            cancellationToken);
    }
}
