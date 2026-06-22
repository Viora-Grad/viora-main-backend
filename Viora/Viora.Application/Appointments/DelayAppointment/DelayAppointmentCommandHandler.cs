using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;

namespace Viora.Application.Appointments.DelayAppointment;

/// <summary>
/// Handles the delay appointment command.This also raises a domain event that can be handled by other parts of the system,
/// such as notifying the customer or updating the staff member's schedule or delaying affected appointments,
/// or logging the delay for auditing purposes
/// </summary>
/// <param name="userContext"> corresponds to the authorized staff member who can access this endpoint and by respect this command</param>
/// <param name="appointmentsRepository"></param>
/// <param name="unitOfWork"></param>
internal class DelayAppointmentCommandHandler(
    IUserContext userContext,
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
        var userId = userContext.UserId;

        appointment.Delay(request.DelayDuration, $"Appointment delayed by: {userId}");
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
