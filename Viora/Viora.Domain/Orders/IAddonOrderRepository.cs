namespace Viora.Domain.Orders;

public interface IAddonOrderRepository
{
    public void Add(AddonOrder order);
    Task<AddonOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<AddonOrder>> GetAllByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken);
}
