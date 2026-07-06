using MediatR;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Appointments.Shared;
using Viora.Application.Wallets.PromisePayment;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Internal;
using Viora.Domain.RealTimeScheduling;
using Viora.Domain.Services;
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
    IServiceRepository serviceRepository,
    ISender sender,
    IUserContext context,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateAppointmentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var userId = context.UserId;
        var orgId = context.OrganizationId;

        Customer? customer = null;
        if (orgId is null)
        {
            customer = await customerRepository.GetByIdAsync(userId, cancellationToken)
                ?? throw new NotFoundException("Customer could not be found.");
        }



        // The branch and duration are properties of the service, not the client request.
        var service = await serviceRepository.GetByIdAsync(request.ServiceId, cancellationToken)
                ?? throw new NotFoundException("Service could not be found.");

        var branchId = service.BranchId;
        var estimatedDuration = service.Duration;

        var schedule = await scheduleRepository.getByBranchIdAndDayAsync(branchId, request.ReservationDate.DayOfWeek, cancellationToken) ??
            throw new NotFoundException("No schedule found for the requested branch and day.");

        var shift = await shiftRepository.GetActiveShiftAsync(schedule.Id,
            request.StaffId,
            TimeOnly.FromDateTime(request.ReservationDate),
            cancellationToken) ?? throw new NotFoundException("No active shift found for the requested staff member at the specified time.");

        var shiftstart = new DateTime(request.ReservationDate.Year, request.ReservationDate.Month, request.ReservationDate.Day, shift.StartTime.Hour, shift.StartTime.Minute, 0);
        var shiftEnd = new DateTime(request.ReservationDate.Year, request.ReservationDate.Month, request.ReservationDate.Day, shift.EndTime.Hour, shift.EndTime.Minute, 0);

        var isWithinShift = request.ReservationDate >= shiftstart && request.ReservationDate.Add(estimatedDuration) <= shiftEnd;
        if (!isWithinShift)
        {
            return Result.Failure<Guid>(AppointmentErrors.AppointmentNotWithinShift);
        }


        var isOverlapping = await appointmentsRepository.OverlapsAsync(
            request.ServiceId,
            request.StaffId,
            request.ReservationDate,
            request.ReservationDate.Add(estimatedDuration),
            cancellationToken); // check for overlapping appointments

        if (isOverlapping)
        {
            return Result.Failure<Guid>(AppointmentErrors.AppointmentTimeConflict);
        }
        CustomerStatus? status = !string.IsNullOrEmpty(request.Status) ? Enum.Parse<CustomerStatus>(request.Status, true) : null;
        var parameters = new GetAppointmentsParameters(
            BranchId: branchId,
            ServiceId: request.ServiceId,
            FromDate: shiftstart,
            ToDate: request.ReservationDate.Add(estimatedDuration)

            );
        var specs = new GetAppointmentsSpecification(parameters);

        var queueNumber = await appointmentsRepository.CountAsync(specs, cancellationToken);

        var payMethod = Enum.Parse<PaymentMethod>(request.PaymentMethod, true);

        // Wallet payments are settled through an escrow promise: hold the service cost now, and store the
        // resulting hold-transaction id as the appointment's PaymentId. On check-in the promise settles to
        // the branch; if the customer never shows, it expires and refunds. If this appointment insert then
        // fails, the promise simply expires and auto-refunds (self-healing).
        var paymentId = request.PaymentId;
        if (payMethod == PaymentMethod.Wallet)
        {
            var promiseResult = await sender.Send(
                new PromisePaymentCommand(userId, branchId, service.Cost, request.ReservationDate),
                cancellationToken);

            if (promiseResult.IsFailure)
                return Result.Failure<Guid>(promiseResult.Error);

            paymentId = promiseResult.Value;
        }
        Guid? customerId;
        if (customer is not null)
        {
            customerId = customer.Id;
        }
        else
        {
            customerId = null;
        }

        var appointment = Appointment.Book(
            customerId,
            request.ServiceId,
            request.StaffId,
            branchId,
            paymentId,
            request.ReservationDate,
            (int)queueNumber,
            payMethod,
            status,
            Enum.Parse<Creator>(request.CreatedBy, true),
            Enum.Parse<Platform>(request.RequestPlatform, true),
            (int)estimatedDuration.TotalMinutes,
            dateTimeProvider.UtcNow);

        appointmentsRepository.Add(appointment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(appointment.Id);

    }
}