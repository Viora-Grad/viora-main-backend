using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Staffs;

namespace Viora.Application.Appointments.NoShowAppointment;

internal class NoShowAppointmentCommandHandler(
    IUserContext userContext,
    IStaffRepository staffRepository,
    IAppointmentsRepository appointmentsRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider
    ) : ICommandHandler<NoShowAppointmentCommand>
{
    public async Task<Result> Handle(NoShowAppointmentCommand request, CancellationToken cancellationToken)
    {
        var staffId = userContext.UserId;
        var staff = await staffRepository.GetByIdAsync(staffId, cancellationToken) ??
            throw new NotFoundException("Staff with ID " + staffId + " not found.");

        var appointment = await appointmentsRepository.GetByIdAsync(request.AppointmentId, cancellationToken) ??
            throw new NotFoundException("Appointment with ID " + request.AppointmentId + " not found.");

        var noShowResult = appointment.NoShow(dateTimeProvider.UtcNow);

        if (noShowResult.IsFailure)
        {
            return Result.Failure(noShowResult.Error);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
