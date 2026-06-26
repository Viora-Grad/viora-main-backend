using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Internal;
using Viora.Domain.Staffs;

namespace Viora.Application.Appointments.CheckInAppointment;

internal class CheckInAppointmentCommandHandler(
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    IAppointmentsRepository appointmentsRepository,
    IDateTimeProvider dateTimeProvider,
    IStaffRepository staffRepository
    ) : ICommandHandler<CheckInAppointmentCommand>
{
    public async Task<Result> Handle(CheckInAppointmentCommand request, CancellationToken cancellationToken)
    {
        var staffId = userContext.UserId;
        var staff = await staffRepository.GetByIdAsync(staffId, cancellationToken) ??
            throw new NotFoundException("Staff with ID " + staffId + " not found.");

        var appointment = await appointmentsRepository.GetByIdAsync(request.AppointmentId, cancellationToken) ??
            throw new NotFoundException("Appointment with ID " + request.AppointmentId + " not found.");

        var checkInResult = appointment.CheckIn(dateTimeProvider.UtcNow, Creator.Staff);

        if (checkInResult.IsFailure)
        {
            return Result.Failure(checkInResult.Error);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

