using Microsoft.EntityFrameworkCore;
using Viora.Domain.Reminders;

namespace Viora.Infrastructure.Repositories.Reminders;

internal class ReminderRepository : Repository<Reminder>, IReminderRepository
{
    public ReminderRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IEnumerable<Reminder>> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken)
    {
        return await DbContext.Set<Reminder>()
            .Where(r => r.AppointmentId == appointmentId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Reminder>> GetByAppointmentsAsync(IEnumerable<Guid> appointmentIds, CancellationToken cancellationToken)
    {
        return await DbContext.Set<Reminder>()
            .Where(r => appointmentIds.Contains(r.AppointmentId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
