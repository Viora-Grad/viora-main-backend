using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;

namespace Viora.Application.Appointments.GetCustomerAppointment;

internal class GetAppointmentQueryHandler(
    IAppointmentsRepository appointmentsRepository) : IQueryHandler<GetAppointmentQuery, Appointment>
{
    public async Task<Result<Appointment>> Handle(GetAppointmentQuery request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentsRepository.GetByIdAsync(request.AppointmentId, cancellationToken) ??
            throw new NotFoundException($"Appointment with id {request.AppointmentId} not found.");
        return Result.Success(appointment);
    }
}
