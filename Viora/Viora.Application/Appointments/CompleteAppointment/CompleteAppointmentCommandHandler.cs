using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;

namespace Viora.Application.Appointments.CompleteAppointment;

internal class CompleteAppointmentCommandHandler(
    IAppointmentsRepository appointmentsRepository,
    IDateTimeProvider dateTimeProvider
    ) : ICommandHandler<CompleteAppointmentCommand>
{
    public async Task<Result> Handle(CompleteAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentsRepository.GetByIdAsync(request.AppointmentId, cancellationToken) ??
            throw new NotFoundException($"Appointment with ID {request.AppointmentId} not found");

        var result = appointment.Complete(dateTimeProvider.UtcNow);

        return result;
    }
}
