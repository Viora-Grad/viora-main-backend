using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Internal;
using Viora.Domain.Staffs;
using Viora.Domain.Users.Customers;

namespace Viora.Application.Appointments.CancelAppointment;

internal class CancelAppointmentCommandHandler(
    IUserContext userContext,
    ICustomerRepository customerRepository,
    IStaffRepository staffRepository,
    IAppointmentsRepository appointmentsRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CancelAppointmentCommand>
{
    public async Task<Result> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentsRepository.GetByIdAsync(request.AppointmentId, cancellationToken) ??
            throw new NotFoundException($"Appointment with ID {request.AppointmentId} not found");

        var customer = await customerRepository.GetByIdAsync(userContext.UserId, cancellationToken);
        if (customer is not null)
        {
            var result = appointment.Cancel(dateTimeProvider.UtcNow, appointment.BranchId, Creator.Customer);

            if (result.IsFailure)
                return Result.Failure(result.Error);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return result;

        }
        var orgId = userContext.OrganizationId;

        if (appointment.Staff.OrganizationId != orgId)
            throw new UnauthorizedAccessException("You are not authorized to cancel this appointment.");


        var staffResult = appointment.Cancel(dateTimeProvider.UtcNow, appointment.BranchId, Creator.Staff);

        if (staffResult.IsFailure)
            return Result.Failure(staffResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();

    }

}
