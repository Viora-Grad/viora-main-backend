using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.RealTimeScheduling.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Branches;
using Viora.Domain.RealTimeScheduling;

namespace Viora.Application.RealTimeScheduling.CancelSchedule;

public class CancelScheduleCommandHandler(
    IShiftRepository shiftRepository,
    IAppointmentsRepository appointmentsRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IScheduleCancellationRepository scheduleCancellationRepository,
    IBranchRepository branchRepository
    ) : ICommandHandler<CancelScheduleCommand>
{
    public async Task<Result> Handle(CancelScheduleCommand request, CancellationToken cancellationToken)
    {
        var branch = await branchRepository.GetByIdAsync(request.branchId, cancellationToken)
            ?? throw new NotFoundException($"Branch with id {request.branchId} not found");

        var shift = await shiftRepository.GetByIdAsync(request.ShiftId, cancellationToken)
            ?? throw new NotFoundException($"shift with id {request.ShiftId} not Found");

        var parameter = new SearchShiftAppoinmentparameter(
            shift.StaffId, DateOnly.FromDateTime(request.date)
            .ToDateTime(shift.StartTime),
            DateOnly.FromDateTime(request.date)
            .ToDateTime(shift.EndTime));

        var spec = new SearchShiftAppointmentSpecification(parameter);

        var shiftAppointment = await appointmentsRepository.ListAsync(spec, cancellationToken);

        var results = shiftAppointment
            .Select(s => s.Cancel(dateTimeProvider.UtcNow, request.branchId));

        var result = results.Any(r => r.IsFailure);

        var cancellation = ScheduleCancellations.Create(request.ShiftId, dateTimeProvider.UtcNow, request.reason);
        scheduleCancellationRepository.Add(cancellation);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
