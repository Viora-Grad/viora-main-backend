using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.RealTimeScheduling.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.RealTimeScheduling;

namespace Viora.Application.RealTimeScheduling.GetStaffShiftByDay;

public class GetStaffShiftByDayQueryHandler(
    /*IBranchRepository branchRepository,
   IStaffRepository staffRepository,*/
    IScheduleRepository scheduleRepository) : IQueryHandler<GetStaffShiftByDayQuery, List<StaffDayShiftResponse>>
{
    public async Task<Result<List<StaffDayShiftResponse>>> Handle(GetStaffShiftByDayQuery request, CancellationToken cancellationToken)
    {
        /*var staff = await staffRepository.GetByIdAsync(request.StaffId, cancellationToken)
               ?? throw new NotFoundException($"Staff with id {request.StaffId} not Found");
           var branch = await branchRepository.GetByIdAsync(staff.BranchId, cancellationToken)
               ?? throw new NotFoundException($"Branch with id {staff.BranchId} not found");*/
        var branchSchedule = await scheduleRepository.getByBranchIdAndDayAsync(branch.Id, request.Date.DayOfWeek, cancellationToken)
            ?? throw new NotFoundException($"branch with Id {branchSchedule.Id} does not havve schedule");
        var staffShifts = branchSchedule
            .Select(
            bs => bs.Intervals.Select(
                s => s.StaffId == request.StaffId &&
                s.DayOfWeek == request.Time.DayOfWeek
                ).ToList()
            ).ToList();

        if (staffShifts is null || !staffShifts.Any())
            return Result.Failure<List<StaffDayShiftResponse>>(ScheduleError.ShiftsNotFound);

        var staffShiftResponse = staffShifts
            .Select(s => new StaffDayShiftResponse(
                s.StaffId,
                s.StartTime,
                s.EndTime,
                s.Appointments.Select(
                    a => a.DateTime
                    )
                )
             ).ToList();

        return Result.Success(staffShiftResponse);

    }
}
