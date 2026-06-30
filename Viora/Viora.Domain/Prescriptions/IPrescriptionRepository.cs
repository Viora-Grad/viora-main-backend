namespace Viora.Domain.Prescriptions;

public interface IPrescriptionRepository
{
    public Task<Prescription?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    public Task<Prescription?> GetByAppointmentIdAsync(Guid id, CancellationToken cancellationToken);
    public void Add(Prescription prescription);
}
