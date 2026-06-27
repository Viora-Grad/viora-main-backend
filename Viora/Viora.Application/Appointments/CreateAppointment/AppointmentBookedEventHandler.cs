using MediatR;
using Viora.Application.Abstractions.Notification;
using Viora.Domain.Appointments.Events;

namespace Viora.Application.Appointments.CreateAppointment;

public class AppointmentBookedEventHandler(
    IScheduleNotifier scheduleNotifier) : INotificationHandler<AppointmentBookedEvent>
{
    public async Task Handle(AppointmentBookedEvent notification, CancellationToken cancellationToken)
    {
        await scheduleNotifier.NotifySlotBookedAsync(
            notification.BranchId,
            notification.AppointmentId,
            notification.ReservationDate,
            ct: cancellationToken);

    }
}
