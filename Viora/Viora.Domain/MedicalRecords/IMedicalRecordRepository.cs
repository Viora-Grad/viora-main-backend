namespace Viora.Domain.MedicalRecords;

public interface IMedicalRecordRepository
{
    void Add(MedicalRecord record);

    public Task<MedicalRecord?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken);
    public Task<MedicalRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
