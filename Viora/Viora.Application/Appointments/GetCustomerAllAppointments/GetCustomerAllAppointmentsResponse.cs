using Viora.Domain.Appointments.Internal;

namespace Viora.Application.Appointments.GetCustomerAppointments;

public sealed record GetCustomerAllAppointmentsResponse
{
    public Guid AppointmentId { get; init; }
    public Guid ServiceId { get; init; }
    public Guid StaffId { get; init; }
    public Guid BranchId { get; init; }
    public DateTime ReservationDate { get; init; }
    public CustomerStatus Status { get; init; }
    public TimeSpan EstimatedDuration { get; init; }
    public string? ServiceName { get; init; }
    public string? StaffName { get; init; }
    public string? Cost { get; init; }
};
