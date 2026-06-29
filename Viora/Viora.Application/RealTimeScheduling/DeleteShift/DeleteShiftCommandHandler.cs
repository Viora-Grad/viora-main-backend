using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Internal;
using Viora.Domain.RealTimeScheduling;

namespace Viora.Application.RealTimeScheduling.DeleteShift;

public class DeleteShiftCommandHandler(
    IShiftRepository shiftRepository,
    IScheduleRepository scheduleRepository,
    IAppointmentsRepository appointmentsRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<DeleteShiftCommand>
{
    public async Task<Result> Handle(DeleteShiftCommand request, CancellationToken cancellationToken)
    {
        var shift = await shiftRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Shift with id {request.Id} not found.");

        var branchSchedule = await scheduleRepository.GetByIdAsync(shift.ScheduleId, cancellationToken)
            ?? throw new NotFoundException($"Schedule with id {shift.ScheduleId} not found.");

        var parameters = new SearchAllShiftAppointmentParameter(shift.StaffId, shift.StartTime, shift.EndTime);

        var specification = new SearchAllShiftAppointmentSpecification(parameters);

        var appointments = await appointmentsRepository.ListAsync(specification, cancellationToken);

        var results = appointments.Select(appointment => appointment.Cancel(dateTimeProvider.UtcNow, branchSchedule.BranchId, Creator.None)).ToList();

        var result = results.Any(
            r => r.IsFailure);

        if (result)
            return Result.Failure(AppointmentErrors.CancellationProhibited);
        shiftRepository.Remove(shift.Id);
        await unitOfWork.SaveChangesAsync();

        return Result.Success();


    }
}
