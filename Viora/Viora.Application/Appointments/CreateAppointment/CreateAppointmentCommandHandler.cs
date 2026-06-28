using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Appointments.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Internal;
using Viora.Domain.RealTimeScheduling;
using Viora.Domain.Users.Customers;

namespace Viora.Application.Appointments.CreateAppointment;
// TODO: Add domain events for appointment creation and handle them in the application layer to send notifications, update staff schedules, etc.
// TODO: Consider a solution for the race condition where two appointments are created at the same time for the same service and staff member.
// This could involve implementing a locking mechanism or using database transactions to ensure data integrity.
internal class CreateAppointmentCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork,
    IAppointmentsRepository appointmentsRepository,
    IScheduleRepository scheduleRepository,
    IShiftRepository shiftRepository,
    IUserContext context,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateAppointmentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var userId = context.UserId;

        var customer = await customerRepository.GetByIdAsync(userId, cancellationToken) ?? throw new NotFoundException("Customer could not be found.");

        var schedule = await scheduleRepository.getByBranchIdAndDayAsync(request.BranchId, request.ReservationDate.DayOfWeek, cancellationToken) ??
            throw new NotFoundException("No schedule found for the requested branch and day.");

        var shift = await shiftRepository.GetActiveShiftAsync(schedule.Id,
            request.StaffId,
            TimeOnly.FromDateTime(request.ReservationDate),
            cancellationToken) ?? throw new NotFoundException("No active shift found for the requested staff member at the specified time.");

        var shiftstart = new DateTime(request.ReservationDate.Year, request.ReservationDate.Month, request.ReservationDate.Day, shift.StartTime.Hour, shift.StartTime.Minute, 0);
        var shiftEnd = new DateTime(request.ReservationDate.Year, request.ReservationDate.Month, request.ReservationDate.Day, shift.EndTime.Hour, shift.EndTime.Minute, 0);

        var isWithinShift = request.ReservationDate >= shiftstart && request.ReservationDate.Add(request.EstimatedDuration) <= shiftEnd;
        if (!isWithinShift)
        {
            return Result.Failure<Guid>(AppointmentErrors.InvalidAppointmentTime);
        }


        var isOverlapping = await appointmentsRepository.OverlapsAsync(
            request.ServiceId,
            request.StaffId,
            request.ReservationDate,
            request.ReservationDate.Add(request.EstimatedDuration),
            cancellationToken); // check for overlapping appointments

        if (isOverlapping)
        {
            return Result.Failure<Guid>(AppointmentErrors.AppointmentTimeConflict);
        }
        CustomerStatus? status = !string.IsNullOrEmpty(request.Status) ? Enum.Parse<CustomerStatus>(request.Status, true) : null;
        var parameters = new GetAppointmentsParameters(
            BranchId: request.BranchId,
            ServiceId: request.ServiceId,
            FromDate: shiftstart,
            ToDate: request.ReservationDate.Add(request.EstimatedDuration)

            );
        var specs = new GetAppointmentsSpecification(parameters);

        var queueNumber = await appointmentsRepository.CountAsync(specs, cancellationToken);

        var payMethod = Enum.Parse<PaymentMethod>(request.PaymentMethod, true);
        var appointment = Appointment.Book(
        userId,
        request.ServiceId,
        request.StaffId,
        request.BranchId,
        request.PaymentId,
        request.ReservationDate,
        (int)queueNumber,
        payMethod,
        status,
        Enum.Parse<Creator>(request.CreatedBy, true),
        Enum.Parse<Platform>(request.RequestPlatform, true),
        request.EstimatedDuration,
        dateTimeProvider.UtcNow);

        appointmentsRepository.Add(appointment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(appointment.Id);

    }
}