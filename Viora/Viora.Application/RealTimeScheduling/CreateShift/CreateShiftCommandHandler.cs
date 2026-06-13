using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.RealTimeScheduling;
using Viora.Domain.Staff;

namespace Viora.Application.RealTimeScheduling.CreateRecurringSchedule;

public class CreateShiftCommandHandler(
    IScheduleRepository scheduleRepository,
    IBranchRepository branchRepository,
    IShiftRepository shiftRepository,
    IUnitOfWork unitOfWork,
    IStaffRepository staffRepository
    ) : ICommandHandler<CreateShiftCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateShiftCommand request, CancellationToken cancellationToken)
    {

        var branch = await branchRepository.GetByIdAsync(request.BranchId, cancellationToken)
            ?? throw new NotFoundException($"Branch with id {request.BranchId} not found.");

        var staff = await staffRepository.GetByIdAsync(request.StaffId, cancellationToken)
            ?? throw new NotFoundException($"Staff with id {request.StaffId} not found");

        var day = Enum.Parse<DayOfWeek>(request.DayOfWeek);

        var branchSchedule = await scheduleRepository.getByBranchIdAndDayAsync(request.BranchId, day, cancellationToken);

        if (branchSchedule is null)
            return Result.Failure<Guid>(ScheduleError.NotFoundForDay);

        var staffshift = branchSchedule.Intervals
            .Where(x => x.StaffId == request.StaffId)
            .ToList();

        var hasOverLap = staffshift.Any(
            x => x.StartTime == request.StartTime &&
            x.EndTime == request.EndTime
            );

        if (hasOverLap)
            return Result.Failure<Guid>(ScheduleError.ShiftOverlap);

        var newShift = Shift.Create(branchSchedule.Id, request.StartTime, request.EndTime, request.StaffId);


        shiftRepository.Add(newShift);
        await unitOfWork.SaveChangesAsync();
        return Result.Success(newShift.Id);

    }
}
