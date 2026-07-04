namespace Viora.Application.Appointments.Shared;

public sealed record AppointmentsResponse
{
    public Guid AppointmentId { get; init; }
    public Guid ServiceId { get; init; }
    public Guid? CustomerId { get; init; }
    public Guid StaffId { get; init; }
    public Guid BranchId { get; init; }
    public Guid? PaymentId { get; init; }
    public DateTime ReservationDate { get; init; }
    public string PaymentMethod { get; init; } = null!;
    public string Status { get; init; } = null!;
    public int EstimatedDurationMinutes { get; init; }
    public string? CustomerName { get; init; }
    public string? ServiceName { get; init; }
    public string? StaffName { get; init; }
    public string? Cost { get; init; }
};
