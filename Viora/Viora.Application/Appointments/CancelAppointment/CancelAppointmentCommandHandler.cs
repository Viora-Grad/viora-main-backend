using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;

namespace Viora.Application.Appointments.CancelAppointment;

internal class CancelAppointmentCommandHandler(
    IAppointmentsRepository appointmentsRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CancelAppointmentCommand>
{
    public async Task<Result> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentsRepository.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment is null)
        {
            return Result.Failure(AppointmentErrors.AppointmentNotFound);
        }
        var now = dateTimeProvider.UtcNow;
        var canCancel = appointment.ReservationDate > now.AddHours(3); // Assuming appointments can only be canceled if they are more than 3 hours away
        if (canCancel)
        {
            appointmentsRepository.Remove(appointment);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        return Result.Failure(AppointmentErrors.CancellationProhibited);
    }
}
