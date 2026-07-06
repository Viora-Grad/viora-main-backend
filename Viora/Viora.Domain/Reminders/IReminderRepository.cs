namespace Viora.Domain.Reminders;

public interface IReminderRepository
{
    void Add(Reminder reminder);
    Task<Reminder?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<Reminder>> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken);
    Task<IEnumerable<Reminder>> GetByAppointmentsAsync(IEnumerable<Guid> appointmentIds, CancellationToken cancellationToken);

}
