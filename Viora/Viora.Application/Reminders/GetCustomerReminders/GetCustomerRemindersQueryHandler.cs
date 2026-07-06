using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Reminders;

namespace Viora.Application.Reminders.GetCustomerReminders;

internal class GetCustomerRemindersQueryHandler(
    IAppointmentsRepository appointmentsRepository,
    IReminderRepository remindersRepository,
    IUserContext userContext
    ) : IQueryHandler<GetCustomerRemindersQuery, IEnumerable<Reminder>>
{
    public async Task<Result<IEnumerable<Reminder>>> Handle(GetCustomerRemindersQuery request, CancellationToken cancellationToken)
    {
        var customerId = userContext.UserId;
        var appointments = await appointmentsRepository.GetByCustomerIdAsync(customerId, cancellationToken);

        if (appointments is null || !appointments.Any())
        {
            return Result.Success(Enumerable.Empty<Reminder>());
        }

        var reminders = await remindersRepository.GetByAppointmentsAsync(appointments.Select(a => a.Id), cancellationToken);
        return Result.Success(reminders);
    }
}
