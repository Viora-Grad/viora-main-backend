namespace Viora.Domain.Organizations;

public interface IOrganizationRepository
{
    public Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
