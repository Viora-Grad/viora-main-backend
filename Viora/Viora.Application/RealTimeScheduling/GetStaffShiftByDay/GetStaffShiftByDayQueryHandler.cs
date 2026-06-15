using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Appointments.AppointmentCompleted;
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
   IScheduleRepository scheduleRepository,
   IShiftRepository shiftRepository) : IQueryHandler<GetStaffShiftByDayQuery, StaffDayShiftResponse>
{
    public async Task<Result<StaffDayShiftResponse>> Handle(GetStaffShiftByDayQuery request, CancellationToken cancellationToken)
    {
        var staff = await staffRepository.GetByIdAsync(request.StaffId, cancellationToken)
              ?? throw new NotFoundException($"Staff with id {request.StaffId} not Found");

        var branch = await branchRepository.GetByIdAsync(staff.BranchId, cancellationToken)
            ?? throw new NotFoundException($"Branch with id {staff.BranchId} not found");

        var branchSchedule = await scheduleRepository.getByBranchIdAndDayAsync(branch.Id, request.Date.DayOfWeek, cancellationToken)
            ?? throw new NotFoundException($"branch with Id {branch.Id} does not have schedule");

        var staffShift = await shiftRepository.GetActiveShiftAsync(branchSchedule.Id, request.StaffId, TimeOnly.FromDateTime(request.Date), cancellationToken)
            ?? throw new NotFoundException($"the staff with id {request.StaffId} does not have shift ");


        var specificationParam = new SearchShiftAppoinmentparameter(
            request.StaffId,
            DateOnly.FromDateTime(request.Date)
            .ToDateTime(staffShift.StartTime),
           DateOnly.FromDateTime(request.Date)
            .ToDateTime(staffShift.EndTime)
        );

        var specification = new SearchShiftAppointmentSpecification(specificationParam);

        var appointments = await appointmentsRepository.ListAsync(specification, cancellationToken);

        var staffShiftResponse = new StaffDayShiftResponse(
            staffShift.Id,
            branchSchedule.Id,
            staffShift.StaffId,
            staffShift.StartTime,
            staffShift.EndTime,
            appointments.Select(appointment => new SlotResponse(
                    appointment.ReservationDate,
                    appointment.EndTime
                        )
                    ).ToList()
            );

        return Result.Success(staffShiftResponse);
    }
}

