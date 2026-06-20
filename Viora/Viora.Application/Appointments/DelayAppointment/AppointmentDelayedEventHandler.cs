using MediatR;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Notification;
using Viora.Application.Notification.NotificationService;
using Viora.Domain.Appointments.Events;
using Viora.Domain.Staff;

namespace Viora.Application.Appointments.DelayAppointment;

public class AppointmentDelayedEventHandler(
    IScheduleNotifier scheduleNotifiy,
    INotificationService notificationService,
    IStaffRepository staffRepository
    ) : INotificationHandler<AppointmentDelayedEvent>
{
    public async Task Handle(AppointmentDelayedEvent notification, CancellationToken cancellationToken)
    {
        var staff = await staffRepository.GetByIdAsync(notification.StaffId, cancellationToken)
            ?? throw new NotFoundException($"the staff wit id {notification.StaffId} not found");

        var newTime = notification.OriginalReservationDate + notification.DelayDuration;

        await scheduleNotifiy.NotifyAppointmentUpdatedAsync(
            staff.BranchId,
            notification.AppointmentId,
            notification.status,
            newTime,
            cancellationToken);

        // send notification 
        await notificationService.SendNotificationAsync(
            notification.CustomerId,
            "appointmentDelay",
            $"your appointment become at {newTime}",
            cancellationToken
            );
    }
}
