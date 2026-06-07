namespace Viora.Application.Appointments.CreateAppointment;
// TODO: Add domain events for appointment creation and handle them in the application layer to send notifications, update staff schedules, etc.
// TODO: Consider a solution for the race condition where two appointments are created at the same time for the same service and staff member.
// This could involve implementing a locking mechanism or using database transactions to ensure data integrity.
/*internal class CreateAppointmentCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork,
    IAppointmentsRepository appointmentsRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateAppointmentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {

        var customer = await customerRepository.GetByIdAsync(request.CustomerId, cancellationToken) ??
            throw new NotFoundException($"Customer with ID {request.CustomerId} not found.");

        var requestedStartTime = request.ReservationDate;
        var requestedEndTime = request.ReservationDate.Add(request.EstimatedDuration);
        var overlaps = await appointmentsRepository.OverlapsAsync(request.ServiceId, request.StaffId, requestedStartTime, requestedEndTime, cancellationToken);

        if (overlaps)
        {
            return Result.Failure<Guid>(AppointmentErrors.AppointmentTimeConflict);
        }
        var appointment = Appointment.Book(
            request.CustomerId,
            request.ServiceId,
            request.StaffId,
            request.ReservationDate,
            request.Status,
            request.CreatedBy,
            request.RequestPlatform,
            request.EstimatedDuration,
            dateTimeProvider.UtcNow);

        appointmentsRepository.Add(appointment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(appointment.Id);
    }
}*/