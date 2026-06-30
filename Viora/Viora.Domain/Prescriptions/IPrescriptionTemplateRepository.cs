namespace Viora.Domain.Prescriptions;

public interface IPrescriptionTemplateRepository
{
    public void Add(PrescriptionTemplate prescription);
    public Task<List<PrescriptionTemplate>> GetByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken);
    public Task<PrescriptionTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
