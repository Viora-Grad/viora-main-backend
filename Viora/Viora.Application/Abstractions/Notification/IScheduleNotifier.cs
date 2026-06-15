namespace Viora.Application.Abstractions.Notification;

public interface IScheduleNotifier
{
    Task NotifyAppointmentUpdatedAsync(
        Guid branchId,
        Guid appointmentId,
        string newStatus,
        DateTime newTime,
        CancellationToken ct);

    Task NotifySlotBookedAsync(
        Guid branchId,
        Guid appointmentId,
        DateTime ReservationDate,
        CancellationToken ct);

    Task NotifySlotFreedAsync(
        Guid branchId,
        Guid appointmentId,
        DateTime freeTime,
        CancellationToken ct);

}
