namespace Viora.Domain.Appointments;

public interface IAppointmentsRepository
{
    Task<IEnumerable<Appointment>> GetAllAsync();
    Task<IEnumerable<Appointment>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByServiceIdAsync(Guid serviceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Appointment appointment);
}
// TODO: Consider using a specification pattern for more complex queries, e.g., GetAppointmentsByCriteriaAsync(AppointmentQueryCriteria criteria)
