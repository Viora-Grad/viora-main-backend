using Viora.Domain.Appointments;

namespace Viora.Infrastructure.Repositories.Appointments;

internal class AppointmentsRepository : Repository<Appointment>, IAppointmentsRepository
{
    public AppointmentsRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
    public Task<IEnumerable<Appointment>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Appointment>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Appointment>> GetByDateRangeAsync(Guid serviceId, Guid staffId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Appointment>> GetByServiceIdAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> OverlapsAsync(Guid serviceId, Guid staffId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
