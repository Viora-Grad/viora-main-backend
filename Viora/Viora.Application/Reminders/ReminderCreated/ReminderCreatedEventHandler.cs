using MediatR;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Notifications.NotificationService;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Notifications;
using Viora.Domain.Notifications.Internal;
using Viora.Domain.Reminders;
using Viora.Domain.Reminders.Events;
using Viora.Domain.Users.Identity;

namespace Viora.Application.Reminders.ReminderCreated;

internal class ReminderCreatedEventHandler(
    INotificationService notificationService,
    IUserRepository userRepository,
    INotificationRepository notificationRepository,
    IAppointmentsRepository appointmentsRepository,
    IReminderRepository reminderRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider
    ) : INotificationHandler<ReminderCreatedEvent>
{
    public async Task Handle(ReminderCreatedEvent notification, CancellationToken cancellationToken)
    {
        var appointment = await appointmentsRepository.GetByIdAsync(notification.AppointmentId, cancellationToken) ??
            throw new NotFoundException($"Appointment with id {notification.AppointmentId} not found.");

        var customerId = appointment.CustomerId ?? throw new NotFoundException($"Customer for appointment with id {notification.AppointmentId} not found.");

        var message = Notification.Create(
            recipientId: customerId,
            title: new Title("Reminder"),
            body: new Body($"You have a reminder for your appointment on {appointment.ReservationDate}."),
            utcNow: dateTimeProvider.UtcNow
        );
        await notificationService.SendNotificationAsync(message, cancellationToken);

        notificationRepository.Add(message);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return;


    }
}
