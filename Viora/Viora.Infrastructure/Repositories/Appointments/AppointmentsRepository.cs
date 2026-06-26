using Microsoft.EntityFrameworkCore;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Infrastructure.Presistance;

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

    public async Task<IReadOnlyList<Appointment>> ListAsync(ISpecification<Appointment> spec, CancellationToken cancellationToken = default)
    {
        return await SpecificationEvaluator<Appointment>
            .GetQuery(DbContext.Set<Appointment>().AsQueryable(), spec)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
    public override async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Appointment>()
            .Include(appointment => appointment.Customer)
            .Include(appointment => appointment.Service)
            .Include(appointment => appointment.Staff)
            .Include(appointment => appointment.Branch)
            .FirstOrDefaultAsync(appointment => appointment.Id == id, cancellationToken);
    }

    public async Task<bool> OverlapsAsync(Guid serviceId, Guid staffId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var overlappingAppointments = await DbContext.Set<Appointment>()
            .Where(appointment => appointment.ServiceId == serviceId &&
            appointment.StaffId == staffId &&
            appointment.ReservationDate < endDate &&
            appointment.ReservationDate.AddMinutes(appointment.EstimatedDuration.Minutes) > startDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return overlappingAppointments.Count != 0;
    }


}
