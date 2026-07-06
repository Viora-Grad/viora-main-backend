namespace Viora.Application.Appointments.GetAppointment;

public class GetAppointmentResponse
{
    public Guid AppointmentId { get; init; }
    public Guid ServiceId { get; init; }
    public Guid? CustomerId { get; init; }
    public Guid StaffId { get; init; }
    public Guid BranchId { get; init; }
    public Guid? PaymentId { get; init; }
    public DateTime ReservationDate { get; init; }
    public string PaymentMethod { get; init; } = null!;
    public bool IsCheckedIn { get; init; }
    public string Status { get; init; } = null!;
    public int EstimatedDurationMinutes { get; init; }
    public DateTime EndTime { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CustomerFirstName { get; init; }
    public string? CustomerLastName { get; init; }
    public string ServiceName { get; init; } = null!;
    public string Cost { get; init; } = null!;
    public string StaffFirstName { get; init; } = null!;
    public string StaffLastName { get; init; } = null!;
    public string StaffPhoneNumber { get; init; } = null!;
    public string Address { get; init; } = null!;

}