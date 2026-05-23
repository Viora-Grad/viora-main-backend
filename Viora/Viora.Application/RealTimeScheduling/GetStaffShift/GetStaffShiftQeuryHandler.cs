using Viora.Application.Abstractions.Messaging;
using Viora.Application.RealTimeScheduling.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.RealTimeScheduling;

namespace Viora.Application.RealTimeScheduling.GetStaffShiftQuery;

public class GetStaffShiftQeuryHandler(
    /*IBranchRepository branchRepository,
    IStaffRepository staffRepository,*/
    IScheduleRepository scheduleRepository
    ) : IQueryHandler<GetStaffShiftQuery, List<StaffShiftResponse>>
{
    public async Task<Result<List<StaffShiftResponse>>> Handle(GetStaffShiftQuery request, CancellationToken cancellationToken)
    {
        /*var staff = await staffRepository.GetByIdAsync(request.StaffId, cancellationToken)
            ?? throw new NotFoundException($"Staff with id {request.StaffId} not Found");
        var branch = await branchRepository.GetByIdAsync(staff.BranchId, cancellationToken)
            ?? throw new NotFoundException($"Branch with id {staff.BranchId} not found");*//*
        var branchSchedule = await scheduleRepository.getByBranchIdAndDayAsync(branch.Id, request.Date.DayOfWeek, cancellationToken)
            ?? throw new NotFoundException($"branch with Id {branchSchedule.Id} does not havve schedule");
        var staffShifts = branchSchedule
            .Select(
            bs => bs.Intervals.Select(
                s => s.StaffId == request.StaffId
                ).ToList()
            ).ToList();

        if (staffShifts is null || !staffShifts.Any())
            return Result.Failure<List<StaffShiftResponse>>(ScheduleError.ShiftsNotFound);

        var shiftResponses = staffShifts.Select(
            staffShifts => staffShifts.Intervals.Select(
                i => new StaffShiftResponse(
                    i.Id,
                    i.StaffId,
                    i.StartTime,
                    i.EndTime,
                    staffShifts.DayOfWeek.ToString()
                    )
                ).ToList()
            ).ToList();

        return Result.Success(shiftResponses);*/
        throw new NotImplementedException();
    }
}

