using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.RealTimeScheduling.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Branches;
using Viora.Domain.RealTimeScheduling;
using Viora.Domain.Staff;
namespace Viora.Application.RealTimeScheduling.GetStaffShiftByDay;

public class GetStaffShiftByDayQueryHandler(
   IBranchRepository branchRepository,
   IAppointmentsRepository appointmentsRepository,
   IStaffRepository staffRepository,
   IScheduleRepository scheduleRepository) : IQueryHandler<GetStaffShiftByDayQuery, List<StaffDayShiftResponse>>
{
    public async Task<Result<List<StaffDayShiftResponse>>> Handle(GetStaffShiftByDayQuery request, CancellationToken cancellationToken)
    {
        var staff = await staffRepository.GetByIdAsync(request.StaffId, cancellationToken)
              ?? throw new NotFoundException($"Staff with id {request.StaffId} not Found");

        var branch = await branchRepository.GetByIdAsync(staff.BranchId, cancellationToken)
            ?? throw new NotFoundException($"Branch with id {staff.BranchId} not found");

        var branchSchedule = await scheduleRepository.getByBranchIdAndDayAsync(branch.Id, request.Date.DayOfWeek, cancellationToken)
            ?? throw new NotFoundException($"branch with Id {branch.Id} does not havve schedule");

        var specificationParam = new SearchStaffAppointmentParameters(
            request.StaffId,
            request.Date
        );

        var specification = new SearchStaffAppointmentspecification(specificationParam);

        var appointments = await appointmentsRepository.ListAsync(specification, cancellationToken);

        var staffShifts = branchSchedule.
            Intervals.Where(s => s.StaffId == request.StaffId).ToList();


        if (staffShifts is null || !staffShifts.Any())
            return Result.Failure<List<StaffDayShiftResponse>>(ScheduleError.ShiftsNotFound);

        var staffShiftResponse = staffShifts
            .Select(s => new StaffDayShiftResponse(
                s.StaffId,
                s.StartTime,
                s.EndTime,
                appointments.Select(appointment => new SlotResponse(
                    appointment.ReservationDate,
                    appointment.EndTime
                        )
                    ).ToList()
                )
             ).ToList();

        return Result.Success(staffShiftResponse);
    }
}

