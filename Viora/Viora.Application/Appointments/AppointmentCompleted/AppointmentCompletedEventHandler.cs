using MediatR;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.RealTimeScheduling.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Events;
using Viora.Domain.RealTimeScheduling;
using Viora.Domain.RealTimeScheduling.Internals;
using Viora.Domain.Staff;

namespace Viora.Application.Appointments.AppointmentCompleted;

public class AppointmentCompletedEventHandler(
    IStaffRepository staffRepository,
    IScheduleDelayRepository scheduleDelayRepository,
    IScheduleRepository scheduleRepository,
    IAppointmentsRepository appointmentsRepository,
    IShiftRepository shiftRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : INotificationHandler<AppointmentCompletedEvent>
{
    public async Task Handle(AppointmentCompletedEvent notification, CancellationToken cancellationToken)
    {
        var appointment = await appointmentsRepository.GetByIdAsync(notification.Id, cancellationToken)
            ?? throw new NotFoundException("the appointment with ID {notification.Id} was not found");

        var delayTime = (notification.CompleteTime - appointment.EndTime).TotalMinutes;

        var staff = await staffRepository.GetByIdAsync(appointment.StaffId, cancellationToken)
            ?? throw new NotFoundException($"the staff with id {appointment.StaffId} not found");

        var branchSchedule = await scheduleRepository.getByBranchIdAndDayAsync(staff.BranchId, notification.CompleteTime.DayOfWeek, cancellationToken)
            ?? throw new NotFoundException($"the branch with id {staff.BranchId}not found");

        var staffShift = await shiftRepository.GetActiveShiftAsync(branchSchedule.Id, staff.Id, TimeOnly.FromDateTime(notification.CompleteTime), cancellationToken)
            ?? throw new NotFoundException($"the staff with id {staff.Id} does not have shift in this day ");

        if (delayTime > 10)
        {
            var parameter = new SearchShiftAppoinmentparameter(appointment.StaffId, appointment.ReservationDate,
                DateOnly.FromDateTime(appointment.ReservationDate).ToDateTime(staffShift.EndTime));

            var specification = new SearchShiftAppointmentSpecification(parameter);

            var shiftAppointments = await appointmentsRepository.ListAsync(specification, cancellationToken);

            var results = shiftAppointments.Select(
                x => x.Delay(TimeSpan.FromMinutes(delayTime))
                );


            var scheduleDelays = shiftAppointments.Select(
                x => ScheduleDelay.Create(
                    x.Id,
                    TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(delayTime)),
                    "delay caused by the previous appointment",
                    dateTimeProvider.UtcNow,
                    InitiatorType.System
                )
            ).ToList();

            scheduleDelayRepository.AddAll(scheduleDelays);
        }



        // transfare the money from the wallet from customer wallet to client wallet 

        await unitOfWork.SaveChangesAsync();

    }
}
