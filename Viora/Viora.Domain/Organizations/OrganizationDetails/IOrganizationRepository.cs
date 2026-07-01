using Viora.Domain.Abstractions;

namespace Viora.Domain.Organizations.OrganizationDetails;

public interface IOrganizationRepository
{
    public void Add(Organization organization);
    public Task<bool> ExistsAsync(Guid Id, CancellationToken cancellationToken = default);
    public Task<bool> NameExistsAsync(string Name, CancellationToken cancellation = default);
    public Task<bool> SubDomainExistsAsync(string subDomain, Guid excludeOrganizationId, CancellationToken cancellationToken = default);
    public Task<Organization?> GetByIdAsync(Guid organization, CancellationToken cancellationToken = default);
    public Task<Organization?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default);
    public Task<Organization?> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<Organization>> ListAsync(ISpecification<Organization> spec, CancellationToken cancellationToken = default);
    public Task<long> CountAsync(ISpecification<Organization> spec, CancellationToken cancellationToken = default);
    public Task<bool> IsOrganizationExistForOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default);
    public Task<Organization?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
