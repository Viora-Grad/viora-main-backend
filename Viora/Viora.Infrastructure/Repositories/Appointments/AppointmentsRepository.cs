using Microsoft.EntityFrameworkCore;
using Viora.Domain.Appointments;

namespace Viora.Infrastructure.Repositories.Appointments;

internal class AppointmentsRepository : Repository<Appointment>, IAppointmentsRepository
{
    public AppointmentsRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
    public Task<IEnumerable<Appointment>> GetAllAsync() // should be an aggregate of appointments to the services the branch/org offers
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Appointment>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Appointment>()
            .Where(appointment => appointment.CustomerId == customerId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByDateRangeAsync(Guid serviceId, Guid staffId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Appointment>()
            .Where(appointment => appointment.ServiceId == serviceId && appointment.StaffId == staffId && appointment.ReservationDate >= startDate && appointment.EndTime <= endDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByServiceIdAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Appointment>()
            .Where(appointment => appointment.ServiceId == serviceId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> OverlapsAsync(Guid serviceId, Guid staffId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var overlappingAppointments = await DbContext.Set<Appointment>()
            .Where(appointment => appointment.ServiceId == serviceId && appointment.StaffId == staffId && appointment.ReservationDate < endDate && appointment.EndTime > startDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return overlappingAppointments.Count != 0;
    }

}
