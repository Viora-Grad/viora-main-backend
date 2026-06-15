using Microsoft.AspNetCore.SignalR;
using Viora.Application.Abstractions.Notification;
using Viora.Infrastructure.RealTime.Hubs;

namespace Viora.Infrastructure.RealTime;

public class ScheduleNotifier : IScheduleNotifier
{
    private readonly IHubContext<ScheduleHub> _hubContext;

    public ScheduleNotifier(IHubContext<ScheduleHub> hubContext) => _hubContext = hubContext;
    public async Task NotifyAppointmentUpdatedAsync(Guid branchId, Guid appointmentId, string newStatus, DateTime newTime, CancellationToken ct)
    {
        await _hubContext.Clients.Group(branchId.ToString()).SendAsync(
           "AppointmentUpdated",
           new
           {
               AppointmentId = appointmentId,
               Status = newStatus,
               TotalDelayMinutes = newTime
           },
           ct);
    }

    public async Task NotifySlotBookedAsync(Guid branchId, Guid appointmentId, DateTime scheduledAt, CancellationToken ct)
    {
        await _hubContext.Clients.Group(branchId.ToString()).SendAsync(
            "SlotBooked",
            new
            {
                AppointmentId = appointmentId,
                ScheduledAt = scheduledAt
            },
            ct);
    }

    public async Task NotifySlotFreedAsync(Guid branchId, Guid appointmentId, CancellationToken ct)
    {
        await _hubContext.Clients.Group(branchId.ToString()).SendAsync(
            "SlotFreed",
            appointmentId,
            ct);
    }
}
