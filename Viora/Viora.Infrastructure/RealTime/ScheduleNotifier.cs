using Viora.Application.Abstractions.Notification;

namespace Viora.Infrastructure.RealTime;

public class ScheduleNotifier : IScheduleNotifier
{
    public Task NotifyAppointmentUpdatedAsync(Guid appointmentId, string newStatus, int totalDelayMinutes, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task NotifySlotBookedAsync(Guid appointmentId, DateTime scheduledAt, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task NotifySlotFreedAsync(Guid appointmentId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
