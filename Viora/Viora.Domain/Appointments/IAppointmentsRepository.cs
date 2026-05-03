namespace Viora.Domain.Appointments;

public interface IAppointmentsRepository
{
    Task<IEnumerable<Appointment>> GetAllAsync();
    Task<IEnumerable<Appointment>> GetByCustomerIdAsync(Guid customerId);
    Task<IEnumerable<Appointment>> GetByServiceIdAsync(Guid serviceId);
    Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Appointment?> GetByIdAsync(Guid id);
    void Add(Appointment appointment);
}
// TODO: Consider using a specification pattern for more complex queries, e.g., GetAppointmentsByCriteriaAsync(AppointmentQueryCriteria criteria)
