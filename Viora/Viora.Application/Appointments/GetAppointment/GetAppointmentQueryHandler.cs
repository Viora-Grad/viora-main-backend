using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;

namespace Viora.Application.Appointments.GetAppointment;

public class GetAppointmentQueryHandler(
    IAppointmentsRepository appointmentsRepository) : IQueryHandler<GetAppointmentQuery, GetAppointmentResponse>
{
    public async Task<Result<GetAppointmentResponse>> Handle(GetAppointmentQuery request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentsRepository.GetByIdAsync(request.AppointmentId, cancellationToken) ??
            throw new NotFoundException($"Appointment with id {request.AppointmentId} not found.");

        var response = new GetAppointmentResponse
        {
            AppointmentId = appointment.Id,
            ServiceId = appointment.ServiceId,
            CustomerId = appointment.CustomerId,
            StaffId = appointment.StaffId,
            BranchId = appointment.BranchId,
            PaymentId = appointment.PaymentId,
            ReservationDate = appointment.ReservationDate,
            PaymentMethod = appointment.PayMethod.ToString(),
            IsCheckedIn = appointment.IsCheckedIn,
            Status = appointment.Status.ToString(),
            EstimatedDurationMinutes = appointment.EstimatedDurationMinutes,
            AppointmentQueueNumber = appointment.AppointmentQueueNumber,
            EndTime = appointment.EndTime,
            CreatedAt = appointment.CreatedAt,
            CustomerFirstName = appointment.Customer?.PersonalInfo.FirstName,
            CustomerLastName = appointment.Customer?.PersonalInfo.LastName,
            ServiceName = appointment.Service.Name,
            Cost = appointment.Service.Cost.ToString(),
            StaffFirstName = appointment.Staff.FirstName,
            StaffLastName = appointment.Staff.LastName,
            StaffPhoneNumber = appointment.Staff.PhoneNumber,
            Address = $"{appointment.Branch.Address.Value}"
        };
        return Result.Success(response);
    }
}
