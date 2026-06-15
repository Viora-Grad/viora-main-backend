using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.RealTimeScheduling.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Branches;
using Viora.Domain.RealTimeScheduling;
using Viora.Domain.Staff;

namespace Viora.Application.RealTimeScheduling.CancelSchedule;

public class CancelScheduleCommandHandler(
    IStaffRepository staffRepository,
    IShiftRepository shiftRepository,
    IAppointmentsRepository appointmentsRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IBranchRepository branchRepository
    ) : ICommandHandler<CancelScheduleCommand>
{
    public async Task<Result> Handle(CancelScheduleCommand request, CancellationToken cancellationToken)
    {
        var staff = await staffRepository.GetByIdAsync(request.StaffId, cancellationToken)
             ?? throw new NotFoundException($"staff with id {request.StaffId} not found ");

        var branch = await branchRepository.GetByIdAsync(request.branchId, cancellationToken)
            ?? throw new NotFoundException($"Branch with id {request.branchId} not found");

        var shift = await shiftRepository.GetByIdAsync(request.ShiftId, cancellationToken)
            ?? throw new NotFoundException($"shift with id {request.ShiftId} not Found");
        var parameter = new SearchShiftAppoinmentparameter(
            request.StaffId, DateOnly.FromDateTime(request.date)
            .ToDateTime(shift.StartTime),
            DateOnly.FromDateTime(request.date)
            .ToDateTime(shift.EndTime));

        var spec = new SearchShiftAppointmentSpecification(parameter);

        var shiftAppointment = await appointmentsRepository.ListAsync(spec, cancellationToken);

        var results = shiftAppointment
            .Select(s => s.Cancel(dateTimeProvider.UtcNow, request.branchId));

        var result = results.Any(r => r.IsFailure);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
