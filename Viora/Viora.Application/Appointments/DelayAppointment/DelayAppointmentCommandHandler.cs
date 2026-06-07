using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;

namespace Viora.Application.Appointments.DelayAppointment;

internal class DelayAppointmentCommandHandler(
    IAppointmentsRepository appointmentsRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DelayAppointmentCommand>
{
    public async Task<Result> Handle(DelayAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentsRepository.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment is null)
        {
            return Result.Failure(AppointmentErrors.AppointmentNotFound);
        }

        appointment.Delay(request.DelayDuration);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
