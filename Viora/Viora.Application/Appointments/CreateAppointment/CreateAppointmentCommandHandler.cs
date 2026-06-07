using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Users.Customers;
using Viora.Domain.Users.Identity;

namespace Viora.Application.Appointments.CreateAppointment;
/// <summary>
/// Important assumptions:<br></br>
/// considering that the customer is the one who creates the appointment, we can assume that if the customer does not exist, we will create a new customer<br></br>
/// assume service id and staff id are valid and exist in the system (not implemented yet, but will be implemented in the future when we have the service and staff modules)
/// 
/// </summary>
internal class CreateAppointmentCommandHandler(
    ICustomerRepository customerRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IAppointmentsRepository appointmentsRepository) : ICommandHandler<CreateAppointmentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        // if the user is not a customer, create a new customer
        var customer = await customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null)
        {

            var user = await userRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (user is null)
            {
                return Result.Failure<Guid>(UserErrors.NotFound);
            }

            var personalInfo = user.PersonalInfo;
            customer = Customer.Create(request.CustomerId, null, personalInfo, dateTimeProvider.UtcNow, null);
            customerRepository.Add(customer);
        }
        var serviceAppointments = await appointmentsRepository.GetByServiceIdAsync(request.ServiceId, cancellationToken);
        var requestedStartTime = request.ReservationDate;
        var requestedEndTime = request.ReservationDate.Add(request.EstimatedDuration);

        var hasConflict = serviceAppointments.Any(appointment =>
        {
            var appointmentStartTime = appointment.ReservationDate;
            var appointmentEndTime = appointment.ReservationDate.Add(appointment.EstimatedDuration);
            return requestedStartTime < appointmentEndTime && requestedEndTime > appointmentStartTime;
        });
        if (hasConflict)
        {
            return Result.Failure<Guid>(AppointmentErrors.AppointmentTimeConflict);
        }
        var validAppointment = dateTimeProvider.UtcNow < request.ReservationDate && request.EstimatedDuration > TimeSpan.Zero && !hasConflict;
        if (validAppointment)
        {
            var appointment = Appointment.Book(request.CustomerId,
                request.ServiceId,
                request.StaffId,
                request.ReservationDate,
                request.Status,
                request.CreatedBy,
                request.RequestPlatform,
                request.EstimatedDuration);

            appointmentsRepository.Add(appointment);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(appointment.Id);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Failure<Guid>(AppointmentErrors.InvalidAppointmentTime);
    }
}
