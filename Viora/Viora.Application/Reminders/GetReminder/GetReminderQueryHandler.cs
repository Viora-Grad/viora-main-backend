using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Reminders;

namespace Viora.Application.Reminders.GetReminder;

internal class GetReminderQueryHandler(IReminderRepository reminderRepository) : IQueryHandler<GetReminderQuery, Reminder>
{
    public async Task<Result<Reminder>> Handle(GetReminderQuery request, CancellationToken cancellationToken)
    {
        var reminder = await reminderRepository.GetByIdAsync(request.ReminderId, cancellationToken) ??
            throw new NotFoundException("Reminder not found");
        return Result.Success(reminder);
    }
}
