namespace Viora.Domain.Orders;

public interface IAddonOrderRepository
{
    public void Add(AddonOrder order);
    Task<List<AddonOrder>> GetAllByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken);
}
