using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Internal;

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
        var orgId = userContext.OrganizationId;

        if (orgId is null || appointment.Staff.OrganizationId != orgId)
        {
            throw new UnauthorizedAccessException("You are not authorized to delay this appointment.");
        }

        // get affected appointments and delay them as well
        var endOfDay = appointment.ReservationDate.Date.AddDays(1).AddTicks(-1);
        var affectedAppointments = await appointmentsRepository.GetByDateRangeAsync(
            appointment.ServiceId,
            appointment.StaffId,
            appointment.ReservationDate,
            endOfDay,
            cancellationToken);

        var sorted = affectedAppointments
            .Where(a => a.Status != CustomerStatus.Canceled)
            .OrderBy(a => a.ReservationDate)
            .ToList();


        appointment.Delay(request.DelayDuration, $"Appointment delayed by: {userId}"); // delay the requested appointment first
        for (int i = 1; i < sorted.Count; i++)
        {
            var prev = sorted[i - 1];
            var current = sorted[i];
            if (current.ReservationDate < prev.EndTime)
            {
                var delayDuration = prev.EndTime - current.ReservationDate;
                current.Delay(delayDuration, $"Appointment delayed due to previous appointment delay by: {userId}");
            }
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
