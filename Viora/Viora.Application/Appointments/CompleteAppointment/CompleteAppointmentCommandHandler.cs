using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Internal;
using Viora.Domain.Staffs;

namespace Viora.Application.Appointments.CompleteAppointment;

public class CompleteAppointmentCommandHandler(
    IUserContext userContext,
    IStaffRepository staffRepository,
    IAppointmentsRepository appointmentsRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork
    ) : ICommandHandler<CompleteAppointmentCommand>
{
    public async Task<Result> Handle(CompleteAppointmentCommand request, CancellationToken cancellationToken)
    {
        var staffId = userContext.UserId;
        var orgId = userContext.OrganizationId;

        var appointment = await appointmentsRepository.GetByIdAsync(request.AppointmentId, cancellationToken) ??
            throw new NotFoundException($"Appointment with ID {request.AppointmentId} not found");

        if (appointment.Staff.OrganizationId != orgId)
        {
            throw new UnauthorizedAccessException($"You are not authorized to complete appointment with ID {appointment.Id}");
        }

        var result = appointment.Complete(dateTimeProvider.UtcNow);

        // complete time may cross another appointment, so we need to shift the other appointments
        if (dateTimeProvider.UtcNow > appointment.EndTime.AddMinutes(20))
        {

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

            for (int i = 1; i < sorted.Count; i++)
            {
                var prev = sorted[i - 1];
                var current = sorted[i];
                var currentCompleteTime = new[] { current.ReservationDate, current.LastUpdatedAt ?? current.ReservationDate }.Max();

                if (currentCompleteTime < prev.EndTime)
                {
                    var delayDuration = prev.EndTime - currentCompleteTime;
                    current.Delay(delayDuration, $"Appointment delayed due to previous appointment delayed Completion");
                }
            }
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();

        }
        else
            await unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }
}
