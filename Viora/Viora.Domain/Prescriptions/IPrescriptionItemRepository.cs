namespace Viora.Domain.Prescriptions;

public interface IPrescriptionItemRepository
{
    public void AddRange(IEnumerable<PrescriptionItem> prescriptions);
}
