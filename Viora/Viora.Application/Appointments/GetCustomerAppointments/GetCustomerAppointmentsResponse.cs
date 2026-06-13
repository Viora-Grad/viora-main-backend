using Viora.Domain.Appointments.Internal;

namespace Viora.Application.Appointments.GetCustomerAppointments;

public sealed record GetCustomerAppointmentsResponse(
    Guid AppointmentId,
    Guid ServiceId,
    Guid StaffId,
    DateTime ReservationDate,
    CustomerStatus Status,
    TimeSpan EstimatedDuration,
    string ServiceName,
    string StaffName,
    string Cost
);
