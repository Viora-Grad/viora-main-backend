namespace Viora.Api.Controllers.Appointments;

public sealed record CreateAppointmentRequest(
    Guid ServiceId,
    Guid StaffId,
    Guid BranchId,
    Guid? PaymentId,
    DateTime ReservationDate,
    string PaymentMethod,
    string? Status,
    string CreatedBy,
    string RequestPlatform,
    TimeSpan EstimatedDuration);