using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Scheduling;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Internal;
using Viora.Domain.Reminders;
using Viora.Domain.Reminders.Events;

namespace Viora.Application.Reminders.CreateReminder;

internal class CreateReminderCommandHandler(
    IAppointmentsRepository appointmentsRepository,
    IReminderRepository reminderRepository,
    IDomainEventScheduler scheduler,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork
    ) : ICommandHandler<CreateReminderCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateReminderCommand request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentsRepository.GetByIdAsync(request.AppointmentId, cancellationToken) ??
            throw new NotFoundException("Appointment not found");

        if (appointment.Status != CustomerStatus.Completed)
            throw new ConflictException("Cannot create a reminder for an appointment that is not completed");

        var reminder = Reminder.Create(
            request.AppointmentId,
            request.Title,
            request.Body,
            dateTimeProvider.UtcNow,
            request.ScheduledFor
        );
        reminderRepository.Add(reminder);
        await scheduler.ScheduleAsync(
            new ReminderCreatedEvent(reminder.Id, appointment.Id),
            reminder.ScheduledFor,
            cancellationToken
            );
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(reminder.Id);

    }
}
