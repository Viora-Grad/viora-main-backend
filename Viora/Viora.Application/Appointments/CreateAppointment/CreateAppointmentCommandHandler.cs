using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Users.Customers;

namespace Viora.Application.Appointments.CreateAppointment;
// TODO: Add domain events for appointment creation and handle them in the application layer to send notifications, update staff schedules, etc.
// TODO: Consider a solution for the race condition where two appointments are created at the same time for the same service and staff member.
// This could involve implementing a locking mechanism or using database transactions to ensure data integrity.
internal class CreateAppointmentCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork,
    IAppointmentsRepository appointmentsRepository,
    // IShiftRepository shiftRepository, // Consider adding a shift repository to check staff availability before creating an appointment
    IUserContext context,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateAppointmentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var userId = context.UserId;

        var customer = await customerRepository.GetByIdAsync(userId, cancellationToken) ?? throw new NotFoundException("Customer could not be found.");

        /* var shift = await shiftRepository.GetShiftForStaffAsync(request.StaffId, request.ReservationDate, cancellationToken); // get the staff member's shift for the reservation date
         if (shift == null)
         {
             throw new NotFoundException("Staff member is not available for the requested time.");
         }
        var shiftstart = DateTime(Request.ReservationDate.Year, Request.ReservationDate.Month, Request.ReservationDate.Day, shift.StartTime.Hours, shift.StartTime.Minutes, 0);
        var shiftEnd = DateTime(Request.ReservationDate.Year, Request.ReservationDate.Month, Request.ReservationDate.Day, shift.EndTime.Hours, shift.EndTime.Minutes, 0);

        var isOverlapping = await appointmentsRepository.IsOverlappingAsync(request.ServiceId, request.StaffId, shiftstart, shiftEnd, cancellationToken); // check for overlapping appointments
        if (isOverlapping)
        {
            throw new InvalidOperationException("The staff member already has an appointment during the requested time.");
        } 
        var appointment = Appointment.Create(
        Guid.NewGuid(), 
        request.ServiceId,
        request.StaffId,
        request.PaymentId,
        request.ReservationDate,
        request.Status,
        request.CreatedBy,
        request.RequestPlatform,
        request.EstimatedDuration);
        
        appointmentsRepository.Add(appointment);
        unitOfWork.SaveChanges(cancellationToken);
        return Result.Success(appointment.Id);
        */
        throw new NotImplementedException();
    }
}