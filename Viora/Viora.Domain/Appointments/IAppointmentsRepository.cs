using Viora.Domain.Abstractions;

namespace Viora.Domain.Appointments;

public interface IAppointmentsRepository
{
    Task<IEnumerable<Appointment>> GetAllAsync(); // Consider adding pagination parameters for large datasets and no tracking for read-only queries
    Task<IEnumerable<Appointment>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByServiceIdAsync(Guid serviceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<bool> OverlapsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Appointment appointment);
    void Remove(Appointment appointment);

    Task<IReadOnlyList<Appointment>> ListAsync(ISpecification<Appointment> spec, CancellationToken cancellationToken = default);
}
// TODO: Consider using a specification pattern for more complex queries, e.g., GetAppointmentsByCriteriaAsync(AppointmentQueryCriteria criteria)
