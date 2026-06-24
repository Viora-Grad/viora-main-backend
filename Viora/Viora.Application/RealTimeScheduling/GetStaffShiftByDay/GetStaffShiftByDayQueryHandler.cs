using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.RealTimeScheduling.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.RealTimeScheduling;
using Viora.Domain.Staffs;
namespace Viora.Application.RealTimeScheduling.GetStaffShiftByDay;

public class GetStaffShiftByDayQueryHandler(
   IAppointmentsRepository appointmentsRepository,
   IStaffRepository staffRepository,
   IShiftRepository shiftRepository) : IQueryHandler<GetStaffShiftByDayQuery, StaffDayShiftResponse>
{
    public async Task<Result<StaffDayShiftResponse>> Handle(GetStaffShiftByDayQuery request, CancellationToken cancellationToken)
    {
        var staff = await staffRepository.GetByIdAsync(request.StaffId, cancellationToken)
              ?? throw new NotFoundException($"Staff with id {request.StaffId} not Found");

        var staffShift = await shiftRepository.GetByIdAsync(request.ShiftId, cancellationToken)
            ?? throw new NotFoundException($"Shift with id {request.ShiftId} not Found");

        var specificationParam = new SearchStaffAppointmentParameters(
            request.StaffId,
            DateOnly.FromDateTime(request.Date)
            .ToDateTime(staffShift.StartTime),
           DateOnly.FromDateTime(request.Date)
            .ToDateTime(staffShift.EndTime)
        );

        var specification = new SearchStaffAppointmentspecification(specificationParam);

        var appointments = await appointmentsRepository.ListAsync(specification, cancellationToken);

        var staffShiftResponse = new StaffDayShiftResponse(
            staffShift.Id,
            staff.BranchId,
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

