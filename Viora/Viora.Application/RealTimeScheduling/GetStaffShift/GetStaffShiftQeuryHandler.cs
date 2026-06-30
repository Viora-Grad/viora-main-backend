using Viora.Application.Abstractions.Messaging;
using Viora.Application.RealTimeScheduling.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.RealTimeScheduling;
using Viora.Domain.Staffs;

namespace Viora.Application.RealTimeScheduling.GetStaffShiftQuery;

public class GetStaffShiftQeuryHandler(
    IBranchRepository branchRepository,
    IStaffRepository staffRepository,
    IScheduleRepository scheduleRepository
    ) : IQueryHandler<GetStaffShiftQuery, List<StaffShiftResponse>>
{
    public async Task<Result<List<StaffShiftResponse>>> Handle(GetStaffShiftQuery request, CancellationToken cancellationToken)
    {/*
        var staff = await staffRepository.GetByIdAsync(request.StaffId, cancellationToken)
            ?? throw new NotFoundException($"Staff with id {request.StaffId} not Found");

        var branch = await branchRepository.GetByIdAsync(request.BranchId, cancellationToken)
            ?? throw new NotFoundException($"Branch with id {request.BranchId} not found");

        var branchSchedules = await scheduleRepository.getByBranchIdAsync(branch.Id, cancellationToken)
            ?? throw new NotFoundException($"branch with Id {branch.Id} does not havve schedule");

        var staffShiftsResponse = branchSchedules
            .SelectMany(
            bs => bs.Intervals
                    .Where(i => i.StaffId == request.StaffId)
                    .Select(i => new StaffShiftResponse(
                        Guid.NewGuid(),
                        i.StaffId,
                        i.StartTime,
                        i.EndTime,
                        bs.DayOfWeek.ToString()
                    ))).ToList();

        if (staffShiftsResponse is null || !staffShiftsResponse.Any())
            return Result.Failure<List<StaffShiftResponse>>(ScheduleError.ShiftsNotFound);


        return Result.Success(staffShiftsResponse);*/
        throw new NotImplementedException(); // untill staff gets implemented
    }
}

