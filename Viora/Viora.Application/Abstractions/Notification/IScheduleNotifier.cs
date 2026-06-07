namespace Viora.Application.Abstractions.Notification;

public interface IScheduleNotifier
{
    Task NotifyAppointmentUpdatedAsync(
        Guid appointmentId,
        string newStatus,
        int totalDelayMinutes,
        CancellationToken ct);

    Task NotifySlotBookedAsync(
        Guid appointmentId,
        DateTime scheduledAt,
        CancellationToken ct);

    Task NotifySlotFreedAsync(
        Guid appointmentId,
        CancellationToken ct);

}
